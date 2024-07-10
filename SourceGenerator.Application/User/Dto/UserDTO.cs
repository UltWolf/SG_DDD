
namespace SourceGenerator.Domain.User.Entity.DTOs
{
    public record UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}