
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SourceGenerator.Domain.User.Entity.Constants; 

namespace SourceGenerator.Domain.User.Entity.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable(UserConstants.TABLE_NAME);
 
        }
    }
}
