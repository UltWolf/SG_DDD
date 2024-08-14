using Microsoft.CodeAnalysis;
using SourceGenerator.Common.Data;

namespace SourceGenerator.Common.Helper
{
    public class CodeGenerationHelper
    {


        public static void WriteGeneratedClasses(List<ClassesToInsert> classesToInsert, bool force = false)
        {
            foreach (var classToInsert in classesToInsert)
            {
                foreach (var generatedClass in classToInsert.GeneratedClasses)
                {
                    if (!string.IsNullOrEmpty(generatedClass.Generated))
                    {
                        FileHelper.WriteToFile(generatedClass.PathToOutput, generatedClass.ClassName, generatedClass.Generated, force);
                    }
                }
            }
        }
        public static string GetTypeOfId(IPropertySymbol idType)
        {
            return idType.Type.Name;
        }
        public static (INamedTypeSymbol baseFilterClass, string namespaceName) GetBaseFilterClass(IAssemblySymbol assemblySymbol)
        {
            var baseFilterType = assemblySymbol.GlobalNamespace.GetTypeMembers().FirstOrDefault(t => t.Name == "BaseFilter");
            if (baseFilterType != null)
            {
                return (baseFilterType, baseFilterType.ContainingNamespace.ToDisplayString());
            }
            return (null, null);
        }
        //public static IEnumerable<INamedTypeSymbol> GetBaseFilterClasses(IAssemblySymbol assemblySymbol)
        //{
        //    var baseFilterType = assemblySymbol.GlobalNamespace.GetTypeMembers().FirstOrDefault(t => t.Name == "BaseFilter");
        //    if (baseFilterType == null)
        //    {
        //        yield break;
        //    }

        //    foreach (var typeSymbol in assemblySymbol.GlobalNamespace.GetTypeMembers())
        //    {
        //        if (typeSymbol.BaseType?.Equals(baseFilterType) == true)
        //        {
        //            yield return typeSymbol;
        //        }
        //    }
        //}
    }
}
