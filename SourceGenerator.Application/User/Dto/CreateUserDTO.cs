
namespace SourceGenerator.Domain.User.Entity.DTOs
{
    public record CreateUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}