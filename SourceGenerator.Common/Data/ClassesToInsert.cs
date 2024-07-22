using System.Collections.Generic;

namespace SourceGenerator.Common.Data
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
