
namespace SourceGenerator.Domain.User.Entity.AutoMapperConfigs.AutoMapperConfigs
{
    public partial class UserAutoMapperConfigAutoMapperConfig : Profile
    {
        public UserAutoMapperConfigAutoMapperConfig()
        {
            CreateMap<UserAutoMapperConfig, UserAutoMapperConfigDTO>();
            CreateMap<UserAutoMapperConfigDTO, UserAutoMapperConfig>();
        }
    }
}