using Newtonsoft.Json.Linq;

namespace tbc.Services.CodeGen
{
    /// <summary>
    /// Модель одной ноды для генерации кода.
    /// Собирает: VarName, ConstructorArgs, лог-флаги, saveToDb, delete/duration и список кнопок (для ActionNode).
    /// </summary>
    internal class TemplateNode
    {
        public string Id { get; }
        public string Type { get; }
        public string VarName { get; }
        public string ConstructorArgs { get; set; }

        public bool HasLogUsage { get; }
        public bool LogUsage { get; }
        public bool HasNotifyAdmin { get; }
        public bool NotifyAdmin { get; }

        public bool HasSaveToDb { get; }
        public bool SaveToDb { get; }

        public bool HasDelete { get; }
        public bool DeleteAfter { get; }
        public bool HasDuration { get; }
        public int DurationSeconds { get; }
        public bool HasColumnLayout { get; }
        public bool ColumnLayout { get; }

        /// <summary> Для ActionNode: список пар (Label, VarName) дочерних кнопок. </summary>
        public List<(string Label, string VarName)> Buttons { get; } = new();

        public TemplateNode(Node n)
        {
            Id = n.Id;
            Type = n.Type;
            VarName = MakeVarName(Type, Id);

            var data = (JObject)n.Data;

            HasLogUsage = data.ContainsKey("logUsage");
            LogUsage = data["logUsage"]?.Value<bool>() ?? false;
            HasNotifyAdmin = data.ContainsKey("notifyAdmin");
            NotifyAdmin = data["notifyAdmin"]?.Value<bool>() ?? false;

            HasSaveToDb = data.ContainsKey("saveToDb");
            SaveToDb = data["saveToDb"]?.Value<bool>() ?? false;

            HasDelete = data.ContainsKey("delete");
            DeleteAfter = data["delete"]?.Value<bool>() ?? false;
            HasDuration = data.ContainsKey("duration");
            DurationSeconds = int.TryParse(
                data["duration"]?.Value<string>(),
                out var dsec) ? dsec : 0;
            HasColumnLayout = data.ContainsKey("ColumnLayout");
            ColumnLayout = data["ColumnLayout"]?.Value<bool>() ?? false;

            // Базовые аргументы конструктора
            switch (Type)
            {
                case "StartNode":
                    var cmd = data["command"]?.Value<string>() ?? "";
                    ConstructorArgs = $"\"{Escape(cmd)}\"";
                    break;

                case "TextNode":
                    var txt = data["label"]?.Value<string>() ?? "";
                    ConstructorArgs = $"\"{Escape(txt)}\"";
                    break;

                case "DelayNode":
                    var durStr = data["duration"]?.Value<string>()
                                 ?? data["label"]?.Value<string>()
                                 ?? "0";
                    ConstructorArgs = int.TryParse(durStr, out var sec)
                        ? sec.ToString()
                        : "0";
                    break;

                case "ButtonNode":
                    var lbl = data["label"]?.Value<string>() ?? "";
                    ConstructorArgs = $"\"{Escape(lbl)}\"";
                    break;

                case "ActionNode":
                    var act = data["label"]?.Value<string>() ?? "";
                    ConstructorArgs = $"\"{Escape(act)}\", new List<(string, INode?)>()";
                    break;

                case "QuestionNode":
                    var qtxt = data["label"]?.Value<string>() ?? "";
                    ConstructorArgs = $"\"{Escape(qtxt)}\"";
                    break;

                case "KeywordFilterNode":
                    var keys = data["keywords"]?.Value<string>() ?? "";
                    ConstructorArgs = $"\"{Escape(keys)}\"";
                    break;

                default:
                    ConstructorArgs = "";
                    break;
            }
        }

        private static string MakeVarName(string type, string id)
            => char.ToLower(type[0]) + type.Substring(1) + id;

        private static string Escape(string s)
    => s.Replace("\\", "\\\\")        // обратные слэши
         .Replace("\"", "\\\"")       // кавычки
         .Replace("\r", "")           // CR выкидываем
         .Replace("\n", "\\n");       // LF → \n
    }
}
