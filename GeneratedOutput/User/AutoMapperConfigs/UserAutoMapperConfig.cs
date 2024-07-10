
namespace SourceGenerator.Domain.User.Entity.AutoMapperConfigs
{
    public partial class UserAutoMapperConfig : Profile
    {
        public UserAutoMapperConfig()
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
        }
    }
}