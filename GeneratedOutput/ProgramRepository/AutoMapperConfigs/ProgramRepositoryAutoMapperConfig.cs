
namespace GlobalNamespace.Repositories.AutoMapperConfigs
{
    public partial class ProgramRepositoryAutoMapperConfig : Profile
    {
        public ProgramRepositoryAutoMapperConfig()
        {
            CreateMap<ProgramRepository, ProgramRepositoryDTO>();
            CreateMap<ProgramRepositoryDTO, ProgramRepository>();
        }
    }
}