
namespace SourceGenerator.Domain.User.Entity.EFConfigurations.AutoMapperConfigs
{
    public partial class UserEFConfigAutoMapperConfig : Profile
    {
        public UserEFConfigAutoMapperConfig()
        {
            CreateMap<UserEFConfig, UserEFConfigDTO>();
            CreateMap<UserEFConfigDTO, UserEFConfig>();
        }
    }
}