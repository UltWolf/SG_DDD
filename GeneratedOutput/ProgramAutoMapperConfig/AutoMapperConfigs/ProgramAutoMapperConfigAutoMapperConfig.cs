
namespace GlobalNamespace.AutoMapperConfigs.AutoMapperConfigs
{
    public partial class ProgramAutoMapperConfigAutoMapperConfig : Profile
    {
        public ProgramAutoMapperConfigAutoMapperConfig()
        {
            CreateMap<ProgramAutoMapperConfig, ProgramAutoMapperConfigDTO>();
            CreateMap<ProgramAutoMapperConfigDTO, ProgramAutoMapperConfig>();
        }
    }
}