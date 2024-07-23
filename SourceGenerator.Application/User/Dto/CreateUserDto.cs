
namespace SourceGenerator..User.Entity.Application.Dto
{
    public record CreateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}