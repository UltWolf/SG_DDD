using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Common.Helper
{
    public class ClassDeclarationHelper
    {
        public static IEnumerable<INamedTypeSymbol> GetClassDeclarations(INamespaceSymbol namespaceSymbol, string codeAttribute)
        {
            var classDeclarations = new List<INamedTypeSymbol>();

            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (member is INamespaceSymbol nestedNamespace)
                {
                    // Recursively get class declarations from nested namespaces
                    classDeclarations.AddRange(GetClassDeclarations(nestedNamespace, codeAttribute));
                }
                else if (member is INamedTypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Class)
                {
                    if (member.Name != "<Module>" && HasGenerateCodeAttribute(typeSymbol, codeAttribute))

                    {
                        //var properties = GetProperties(typeSymbol);
                        classDeclarations.Add(typeSymbol);
                    }
                }
            }

            return classDeclarations;
        }
        public static string GetModifiedRegion(string codeToInsert, string regionName, SyntaxNode root, GeneratorExecutionContext context, string assemblyName)
        {
            var region = root.DescendantTrivia()
                          .Where(trivia => trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RegionDirectiveTrivia))
                          .FirstOrDefault(trivia => trivia.ToString().Contains(regionName));

            if (region != default)
            {
                var fullText = root.ToFullString();


                var regionSpan = region.Span;
                var regionText = fullText.Substring(regionSpan.Start, regionSpan.Length);
                if (!regionText.Contains(codeToInsert))
                {
                    var newRegionText = regionText + "\n" + codeToInsert;
                    fullText = fullText.Remove(regionSpan.Start, regionSpan.Length).Insert(regionSpan.Start, newRegionText);
                    string usingModule = $"using {assemblyName}.Modules;";
                    if (!fullText.Contains(usingModule))
                    {
                        fullText = fullText.Insert(0, usingModule);
                    }
                    return fullText;
                }
            }
            return null;
        }
        public static string GetNameForClass(INamedTypeSymbol classDeclaration)
        {
            return classDeclaration.Name.Replace("Entity", "");
        }
        private static bool HasGenerateCodeAttribute(INamedTypeSymbol classDeclaration, string attributeValue)
        {
            var generateCodeAttribute = classDeclaration.GetAttributes().FirstOrDefault(attr =>
                attr.AttributeClass.ToDisplayString().Contains("Data.Attributes.GenerateCode")
                 &&
                attr.ConstructorArguments[0].Value is string value &&
                value == attributeValue);

            return generateCodeAttribute != null;
        }
        private IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers().OfType<IPropertySymbol>();
        }
        public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarations(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        }
    }
}
