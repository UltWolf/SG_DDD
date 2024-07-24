namespace SourceGenerator.DomainLevel.Generators.Repository.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateRepositoryAttribute : Attribute
    {
        public Type Context;
        public Type BaseRepository;
        public Type StrongTypeId;
        public Type StrongTypeIdConvertor;

        public GenerateRepositoryAttribute(Type context, Type baseRepository)
        {
            // Validate that the provided type inherits from DbContext
            if (!IsSubclassOf(context, "Microsoft.EntityFrameworkCore.DbContext"))
            {
                throw new ArgumentException($"{context.FullName} does not inherit from DbContext");
            }
            Context = context;
            BaseRepository = baseRepository;
        }
        public GenerateRepositoryAttribute(Type context, Type baseRepository, Type strongTypeId, Type strongTypeIdConvertor) : this(context, baseRepository)
        {
            StrongTypeId = strongTypeId;
            StrongTypeIdConvertor = strongTypeIdConvertor;
        }

        private bool IsSubclassOf(Type type, string baseTypeName)
        {
            while (type != null && type != typeof(object))
            {
                if (type.FullName == baseTypeName)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }
    }

}
