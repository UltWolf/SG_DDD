
namespace SourceGenerator.Domain.User.Entity.Repositories.AutoMapperConfigs
{
    public partial class UserRepositoryAutoMapperConfig : Profile
    {
        public UserRepositoryAutoMapperConfig()
        {
            CreateMap<UserRepository, UserRepositoryDTO>();
            CreateMap<UserRepositoryDTO, UserRepository>();
        }
    }
}