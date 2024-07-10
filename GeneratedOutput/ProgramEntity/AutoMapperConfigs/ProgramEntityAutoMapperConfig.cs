
namespace GlobalNamespace.Entities.AutoMapperConfigs
{
    public partial class ProgramEntityAutoMapperConfig : Profile
    {
        public ProgramEntityAutoMapperConfig()
        {
            CreateMap<ProgramEntity, ProgramEntityDTO>();
            CreateMap<ProgramEntityDTO, ProgramEntity>();
        }
    }
}