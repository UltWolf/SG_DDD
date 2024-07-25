
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SourceGenerator.Domain.User.Entity.Constants; 

namespace SourceGenerator.Domain.User.Entity.Configurations
{
    public class User : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(UserConstants.TABLE_NAME);
 
        }
    }
}
