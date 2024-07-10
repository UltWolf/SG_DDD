
namespace SourceGenerator.Domain.User.Entity.AutoMapperConfigs
{
    public partial class GenerateCodeAttributeAutoMapperConfig : Profile
    {
        public GenerateCodeAttributeAutoMapperConfig()
        {
            CreateMap<GenerateCodeAttribute, GenerateCodeAttributeDTO>();
            CreateMap<GenerateCodeAttributeDTO, GenerateCodeAttribute>();
        }
    }
}