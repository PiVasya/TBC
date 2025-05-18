namespace tbc.Services.CodeGen
{
    internal class TemplateCommand
    {
        public string CommandText { get; }
        public string NodeVar { get; }

        public TemplateCommand(string cmd, string varName)
        {
            CommandText = cmd;
            NodeVar = varName;
        }
    }
}
