namespace SourceGeneratorLib.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateCodeAttribute : Attribute
    {
        public GenerateCodeAttribute(string target)
        {
            Target = target;
        }

        public string Target { get; }
    }
}

