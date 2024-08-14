#pragma warning disable RS1035
namespace SourceGenerator.Common.Helper
{
    public class FileHelper
    {
        public static void WriteToFile(string directory, string fileName, string content, bool force = false)
        {
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var filePath = Path.Combine(directory, fileName);
            if (File.Exists(filePath) && force == false)
            {
                return;
            }
            File.WriteAllText(filePath, content);
        }
    }
}
