#pragma warning disable RS1035
namespace SourceGenerator.Common.Helper
{
    public class FileHelper
    {
        public static void WriteToFile(string directory, string fileName, string content)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var filePath = Path.Combine(directory, fileName);
            if (File.Exists(filePath))
            {
                return;
            }
            File.WriteAllText(filePath, content);
        }
    }
}
