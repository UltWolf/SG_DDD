
namespace SourceGenerator.Domain.User.Entity.Entities.AutoMapperConfigs
{
    public partial class UserEntityAutoMapperConfig : Profile
    {
        public UserEntityAutoMapperConfig()
        {
            CreateMap<UserEntity, UserEntityDTO>();
            CreateMap<UserEntityDTO, UserEntity>();
        }
    }
}