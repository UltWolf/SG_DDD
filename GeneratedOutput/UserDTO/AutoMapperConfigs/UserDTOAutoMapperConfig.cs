
namespace SourceGenerator.Domain.User.Entity.DTOs.AutoMapperConfigs
{
    public partial class UserDTOAutoMapperConfig : Profile
    {
        public UserDTOAutoMapperConfig()
        {
            CreateMap<UserDTO, UserDTODTO>();
            CreateMap<UserDTODTO, UserDTO>();
        }
    }
}