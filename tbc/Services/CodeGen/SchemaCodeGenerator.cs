using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;

namespace TBC.Services.CodeGen
{
    public static class SchemaCodeGenerator
    {
        /// <summary>
        /// Генерирует из чистого JSON-схемы Program.cs, BotProj.csproj и Dockerfile.
        /// </summary>
        public static (string code, string proj, string docker) GenerateCode(
            string schemaJson,
            string telegramToken,
            long adminChatId,
            string templatesPath)
        {
            Console.WriteLine("[CodeGen] Starting GenerateCode");
            Console.WriteLine($"[CodeGen] Raw schemaJson length = {schemaJson?.Length}");

            // 1) десериализуем схему ----------------------------------------------------------
            var schema = JsonConvert.DeserializeObject<FlowSchema>(schemaJson)
                         ?? throw new InvalidOperationException("Schema is null");
            Console.WriteLine($"[CodeGen] Deserialized FlowSchema: nodes = {schema.Nodes.Count}, edges = {schema.Edges.Count}");

            // 2) TemplateNode для всех нод ----------------------------------------------------
            var allNodes = schema.Nodes.Select(n => new TemplateNode(n)).ToList();
            var nodes = allNodes.Where(n => n.Type != "StartNode").ToList();
            // кнопки заполняет только BFS-алгоритм

            // 3) BFS-заполнение Buttons для ActionNode ---------------------------------------
            foreach (var action in nodes.Where(n => n.Type == "ActionNode"))
            {
                var buttons = new List<(string Label, string NextVar)>();
                var seen = new HashSet<string>();
                var queue = new Queue<string>();

                // стартуем от прямых рёбер к Button-нодам
                foreach (var e in schema.Edges.Where(e => e.Source == action.Id))
                {
                    var child = schema.Nodes.First(x => x.Id == e.Target);
                    if (child.Type == "ButtonNode")
                    {
                        queue.Enqueue(child.Id);
                        seen.Add(child.Id);
                    }
                }

                while (queue.Count > 0)
                {
                    var btnId = queue.Dequeue();
                    var btn = schema.Nodes.First(n => n.Id == btnId);
                    var label = btn.Data["label"]?.Value<string>() ?? "";

                    // найдём переход из Button-ноды к первой не-Button-ноде
                    var nextEdge = schema.Edges
                        .FirstOrDefault(e => e.Source == btnId
                                          && schema.Nodes.First(n => n.Id == e.Target).Type != "ButtonNode");

                    var nextVar = nextEdge != null
                        ? allNodes.First(x => x.Id == nextEdge.Target).VarName
                        : "null";

                    buttons.Add((label, nextVar));

                    // очередь следующих Button-нод
                    foreach (var e2 in schema.Edges.Where(e2 => e2.Source == btnId))
                    {
                        var sub = schema.Nodes.First(n => n.Id == e2.Target);
                        if (sub.Type == "ButtonNode" && seen.Add(sub.Id))
                            queue.Enqueue(sub.Id);
                    }
                }

                action.Buttons.AddRange(buttons.Select(b => (b.Label, b.NextVar)));
            }

            // 4) связи Next (без StartNode и ButtonNode) -------------------------------------
            var links = schema.Edges
                .Where(e =>
                {
                    var srcType = schema.Nodes.First(n => n.Id == e.Source).Type;
                    var dstType = schema.Nodes.First(n => n.Id == e.Target).Type;
                    return srcType != "StartNode"
                        && srcType != "ButtonNode"
                        && dstType != "ButtonNode";
                })
                .Select(e => new TemplateLink(
                    allNodes.First(n => n.Id == e.Source).VarName,
                    allNodes.First(n => n.Id == e.Target).VarName))
                .ToList();

            // 5) команды ----------------------------------------------------------------------
            var commands = new List<TemplateCommand>();
            foreach (var start in schema.Nodes.Where(n => n.Type == "StartNode"))
            {
                var cmdText = ((JObject)start.Data)["command"]?.Value<string>() ?? "";
                if (string.IsNullOrEmpty(cmdText)) continue;
                var edge = schema.Edges.FirstOrDefault(e => e.Source == start.Id);
                if (edge == null) continue;
                var childVar = allNodes.First(x => x.Id == edge.Target).VarName;
                commands.Add(new TemplateCommand(cmdText, childVar));
            }

            // 6) DefaultNode ------------------------------------------------------------------
            var defaultNode = commands.FirstOrDefault(c => c.CommandText.Equals("/start", StringComparison.OrdinalIgnoreCase))?.NodeVar
                           ?? commands.FirstOrDefault()?.NodeVar
                           ?? nodes.First().VarName;

            var initLines = new List<string>();
            foreach (var n in nodes.Where(x => x.Type == "ActionNode" && x.HasColumnLayout))
                initLines.Add($"{n.VarName}.ColumnLayout = {n.ColumnLayout.ToString().ToLowerInvariant()};");

            // 7) рендер Program.tpl -----------------------------------------------------------
            var programTplContent = File.ReadAllText(Path.Combine(templatesPath, "Program.tpl"));
            var programTpl = Template.Parse(programTplContent);
            var programCode = programTpl.Render(new
            {
                TelegramToken = telegramToken,
                AdminChatId = adminChatId,
                Nodes = nodes,
                Links = links,
                Commands = commands,
                DefaultNodeVar = defaultNode,
                InitLines = initLines
            }, member => member.Name);

            // 8) остальные шаблоны ------------------------------------------------------------
            var proj = File.ReadAllText(Path.Combine(templatesPath, "BotProj.csproj.tpl"));
            var docker = File.ReadAllText(Path.Combine(templatesPath, "Dockerfile.tpl"));

            return (programCode, proj, docker);
        }
    }
}
