
namespace SourceGenerator.Application.User.Dto
{
    public record UserDto
    {
        public SourceGenerator.Domain.User.Entity.ValueTypes.UserId Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}