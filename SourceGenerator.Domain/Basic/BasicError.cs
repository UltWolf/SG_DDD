namespace SourceGenerator.Domain.Basic
{
    public class BasicError : Exception
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
