
namespace GlobalNamespace.EFConfigurations.AutoMapperConfigs
{
    public partial class ProgramEFConfigAutoMapperConfig : Profile
    {
        public ProgramEFConfigAutoMapperConfig()
        {
            CreateMap<ProgramEFConfig, ProgramEFConfigDTO>();
            CreateMap<ProgramEFConfigDTO, ProgramEFConfig>();
        }
    }
}