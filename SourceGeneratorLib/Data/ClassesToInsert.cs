namespace SourceGeneratorLib.Data
{
    public class ClassesToInsert
    {
        public string ClassName { get; set; }
        public List<GeneratedClass> GeneratedClasses { get; set; } = new List<GeneratedClass>();
    }
    public class GeneratedClass
    {
        public string ClassName { get; set; }
        public string Generated { get; set; }
        public string PathToOutput { get; set; }
    }
}
