using SourceGenerator.Domain.Database;

namespace SourceGenerator.Domain.Basic
{
    public interface IBasicRepository<T, F, L>
    {
        Task<T> AddAsync(T Entity, CancellationToken token);
        Task<T> DeleteAsync(T Entity, CancellationToken token);
        Task<T> UpdateAsync(T Entity, CancellationToken token);
        Task<T> GetByIdAsync(F Key, CancellationToken token);
        Task<IEnumerable<T>> GetFiltered(L filter, CancellationToken token);
    }
    public class BasicRepository<T, F, L, D> : IBasicRepository<T, F, L>
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

        public Task<T> GetByIdAsync(F Key, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetFiltered(L filter, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
