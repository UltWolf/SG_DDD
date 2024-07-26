using SourceGenerator.Domain.Database;

namespace SourceGenerator.Domain.Basic
{
    public interface IBasicRepository<T, F>
    {
        Task<T> AddAsync(T Entity, CancellationToken token);
        Task<T> DeleteAsync(T Entity, CancellationToken token);
        Task<T> UpdateAsync(T Entity, CancellationToken token);
    }
    public class BasicRepository<T, F, L> : IBasicRepository<T, F>
    {
        public ApplicationDbContext Context { get; set; }
        public BasicRepository(ApplicationDbContext applicationDbContext)
        {
            Context = applicationDbContext;
        }
        public Task<T> AddAsync(T Entity, CancellationToken token)
        {
            return Task.FromResult(Entity);
        }
        public Task<T> DeleteAsync(T Entity, CancellationToken token)
        {
            return Task.FromResult(Entity);
        }
        public Task<T> UpdateAsync(T Entity, CancellationToken token)
        {
            return Task.FromResult(Entity);
        }
    }
}
