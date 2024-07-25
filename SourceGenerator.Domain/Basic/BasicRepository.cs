using SourceGenerator.Domain.Database;

namespace SourceGenerator.Domain.Basic
{
    public interface IBasicRepository<T, F>
    {
        T AddAsync(T Entity, CancellationToken token);
        T DeleteAsync(T Entity, CancellationToken token);
        T UpdateAsync(T Entity, CancellationToken token);
    }
    public class BasicRepository<T, F, L> : IBasicRepository<T, F>
    {
        public ApplicationDbContext Context { get; set; }
        public BasicRepository(ApplicationDbContext applicationDbContext)
        {
            Context = applicationDbContext;
        }
        public T AddAsync(T Entity, CancellationToken token)
        {
            return Entity;
        }
        public T DeleteAsync(T Entity, CancellationToken token)
        {
            return Entity;
        }
        public T UpdateAsync(T Entity, CancellationToken token)
        {
            return Entity;
        }
    }
}
