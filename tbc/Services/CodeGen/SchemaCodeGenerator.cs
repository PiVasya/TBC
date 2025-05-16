// Services/CodeGen/SchemaCodeGenerator.cs
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
        /// <param name="schemaJson">JSON вида {"nodes":[...],"edges":[...]}</param>
        /// <param name="telegramToken">Токен телеграм-бота</param>
        /// <param name="adminChatId">ChatId админа</param>
        /// <param name="templatesPath">Путь к папке Templates (где лежат Program.tpl и др.)</param>
        public static (string code, string proj, string docker) GenerateCode(
            string schemaJson,
            string telegramToken,
            long adminChatId,
            string templatesPath)
        {
            // 1) десериализуем "чистую" схему
            var schema = JsonConvert.DeserializeObject<FlowSchema>(schemaJson)
                         ?? throw new InvalidOperationException("Schema is null");

            // 2) Создаём TemplateNode для всех нод, сразу убираем StartNode из списка нод для шаблона
            var allNodes = schema.Nodes.Select(n => new TemplateNode(n)).ToList();
            var nodes = allNodes
                .Where(n => n.Type != "StartNode")
                .ToList();

            // 3) Вшиваем кнопки в ActionNode...
            foreach (var action in nodes.Where(n => n.Type == "ActionNode"))
            {
                var buttons = new List<(string Label, string NextVar)>();
                var seen = new HashSet<string>();
                var queue = new Queue<string>();

                // прямые дети-кнопки
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

                    // следующий после кнопки (не кнопка)
                    var nextEdge = schema.Edges
                        .FirstOrDefault(e => e.Source == btnId
                                           && schema.Nodes.First(n => n.Id == e.Target).Type != "ButtonNode");
                    var nextVar = nextEdge != null
                        ? allNodes.First(x => x.Id == nextEdge.Target).VarName
                        : "null";

                    buttons.Add((label, nextVar));

                    // вложенные кнопки
                    foreach (var e2 in schema.Edges.Where(e2 => e2.Source == btnId))
                    {
                        var sub = schema.Nodes.First(n => n.Id == e2.Target);
                        if (sub.Type == "ButtonNode" && seen.Add(sub.Id))
                            queue.Enqueue(sub.Id);
                    }
                }

                var ctorList = string.Join(", ",
                    buttons.Select(b => $"(\"{Escape(b.Label)}\",{b.NextVar})"));
                action.ConstructorArgs = action.ConstructorArgs
                    .Replace("/* buttons */", ctorList);
            }

            // 4) связи Next (без StartNode и ButtonNode)
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
                    allNodes.First(n => n.Id == e.Target).VarName
                ))
                .ToList();

            // 5) команды (StartNode → childVar)
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

            // 6) выбираем DefaultNodeVar: сначала ищем "/start", иначе первую команду, иначе первую non-StartNode
            var startCmd = commands
                .FirstOrDefault(c => c.CommandText.Equals("/start", StringComparison.OrdinalIgnoreCase));
            var defaultNode = startCmd?.NodeVar
                           ?? commands.FirstOrDefault()?.NodeVar
                           ?? nodes.First().VarName;

            // 7) рендерим Program.tpl
            var programTplPath = Path.Combine(templatesPath, "Program.tpl");
            var programTpl = Template.Parse(File.ReadAllText(programTplPath));
            if (programTpl.HasErrors)
                throw new InvalidOperationException(
                    "Ошибки в шаблоне Program.tpl:\n" +
                    string.Join("\n", programTpl.Messages.Select(m => m.Message)));

            var programCode = programTpl.Render(new
            {
                TelegramToken = telegramToken,
                AdminChatId = adminChatId,
                Nodes = nodes,
                Links = links,
                Commands = commands,
                DefaultNodeVar = defaultNode
            }, member => member.Name);

            // 8) читаем шаблон проекта и докерфайла
            var projTemplate = File.ReadAllText(Path.Combine(templatesPath, "BotProj.csproj.tpl"));
            var dockerTemplate = File.ReadAllText(Path.Combine(templatesPath, "Dockerfile.tpl"));

            return (
                code: programCode,
                proj: projTemplate,
                docker: dockerTemplate
            );
        }

        private static string Escape(string s)
            => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}