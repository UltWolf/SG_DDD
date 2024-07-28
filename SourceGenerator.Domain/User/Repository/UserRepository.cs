
using SourceGenerator.Domain.Database;
using SourceGenerator.Domain.User.Entity.Filters;
using SourceGenerator.Domain.User.Entity.ValueTypes;
using SourceGenerator.Domain.Basic;
namespace SourceGenerator.Domain.User.Entity.Repositories
{
    public interface IUserRepository : IBasicRepository<UserEntity, UserId,UserFilter>
    { 
    } 

 
    public class UserRepository : BasicRepository<UserEntity,UserId,UserFilter,ApplicationDbContext>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
