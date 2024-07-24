using SourceGenerator.Common.Data.Constants;
using SourceGenerator.Domain.Basic;
using SourceGenerator.Domain.Database;

namespace SourceGenerator.Domain.User.Entity
{
    [SourceGenerator.Common.Data.Attributes.GenerateCode(LayerGenerate.DtoLayer)]
    [SourceGenerator.Common.Data.Attributes.GenerateCode(LayerGenerate.MediatRCommandLayer, typeof(BasicError))]
    [SourceGenerator.Common.Data.Attributes.GenerateCode("EFConfigurations")]
    [SourceGenerator.Common.Data.Attributes.GenerateCode("TableConstants")]
    [SourceGenerator.DomainLevel.Generators.Repository.Attributes.GenerateRepository(
        typeof(ApplicationDbContext),
        typeof(BasicRepository),
        typeof(IStronglyTypeGuidId), typeof(StronglyTypedIdTypeConverter))]
    // [SourceGenerator.Common.Data.Attributes.GenerateCode(LayerGenerate.EFRepositoryLayer)]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public User() { }
    }
}
