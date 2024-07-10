
namespace SourceGenerator.Domain.User.Entity.EFConfigurations
{
    public partial class UserEFConfig : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            // EF configuration code for User entity
        }
    }
}