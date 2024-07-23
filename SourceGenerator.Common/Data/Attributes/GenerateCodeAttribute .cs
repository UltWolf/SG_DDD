namespace SourceGenerator.Common.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateCodeAttribute : Attribute
    {
        public Type AdditionalType;
        public GenerateCodeAttribute(string target)
        {
            Target = target;
        }
        public GenerateCodeAttribute(string target, Type additionalType)
        {
            Target = target;
            AdditionalType = additionalType;
        }
        public string Target { get; }
    }
}

