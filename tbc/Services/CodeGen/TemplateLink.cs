namespace TBC.Services.CodeGen
{
    internal class TemplateLink
    {
        public string ParentVar { get; }
        public string ChildVar { get; }

        public TemplateLink(string parentVar, string childVar)
        {
            ParentVar = parentVar;
            ChildVar = childVar;
        }
    }
}
