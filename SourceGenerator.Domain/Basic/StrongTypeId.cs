namespace SourceGenerator.Domain.Basic
{
    public interface ITypeId<T>
    {
        public T Value { get; }
    }

    public interface IStronglyTypeGuidId : ITypeId<Guid>
    {
    }
}
