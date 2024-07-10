
namespace GlobalNamespace.DTOs.AutoMapperConfigs
{
    public partial class ProgramDTOAutoMapperConfig : Profile
    {
        public ProgramDTOAutoMapperConfig()
        {
            CreateMap<ProgramDTO, ProgramDTODTO>();
            CreateMap<ProgramDTODTO, ProgramDTO>();
        }
    }
}