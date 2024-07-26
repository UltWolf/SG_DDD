using SourceGenerator.Common.Data.Constants;
using SourceGenerator.Domain.Basic;
using SourceGenerator.Domain.Database;
using SourceGenerator.Domain.User.Entity.ValueTypes;

namespace SourceGenerator.Domain.User.Entity
{
    [SourceGenerator.Common.Data.Attributes.GenerateCode(LayerGenerate.DtoLayer)]
    [SourceGenerator.Common.Data.Attributes.GenerateCode(LayerGenerate.ControllerLayer)]
    [SourceGenerator.Common.Data.Attributes.GenerateCode(LayerGenerate.MediatRCommandLayer, typeof(BasicError))]
    [SourceGenerator.DomainLevel.Generators.Repository.Attributes.GenerateRepository(
        typeof(ApplicationDbContext),
        typeof(BasicRepository<object, object, object>),
        typeof(IStronglyTypeGuidId), typeof(StronglyTypedIdTypeConverter<Guid>))]
    // [SourceGenerator.Common.Data.Attributes.GenerateCode(LayerGenerate.EFRepositoryLayer)]
    public class UserEntity
    {
        public UserId Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserEntity() { }
    }
}
