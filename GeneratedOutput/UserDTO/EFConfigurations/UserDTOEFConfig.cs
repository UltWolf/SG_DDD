
namespace SourceGenerator.Domain.User.Entity.DTOs.EFConfigurations
{
    public partial class UserDTOEFConfig : IEntityTypeConfiguration<UserDTOEntity>
    {
        public void Configure(EntityTypeBuilder<UserDTOEntity> builder)
        {
            // EF configuration code for UserDTO entity
        }
    }
}