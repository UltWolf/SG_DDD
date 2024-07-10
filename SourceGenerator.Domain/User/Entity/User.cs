namespace SourceGenerator.Domain.User.Entity
{
    [SourceGeneratorLib.Data.Attributes.GenerateCode("DTO")]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public User() { }
    }
}
