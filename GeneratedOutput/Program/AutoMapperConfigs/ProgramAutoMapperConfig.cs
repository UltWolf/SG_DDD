
namespace GlobalNamespace.AutoMapperConfigs
{
    public partial class ProgramAutoMapperConfig : Profile
    {
        public ProgramAutoMapperConfig()
        {
            CreateMap<Program, ProgramDTO>();
            CreateMap<ProgramDTO, Program>();
        }
    }
}