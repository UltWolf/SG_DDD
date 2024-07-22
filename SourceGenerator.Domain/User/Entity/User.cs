namespace SourceGenerator.Domain.User.Entity
{
    [SourceGenerator.Common.Data.Attributes.GenerateCode("DTO")]
    [SourceGenerator.Common.Data.Attributes.GenerateCode("MediatRCommand")]
    [SourceGenerator.Common.Data.Attributes.GenerateCode("EFConfigurations")]
    [SourceGenerator.Common.Data.Attributes.GenerateCode("TableConstants")]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public User() { }
    }
}
