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
        private static bool HasGenerateCodeAttribute(INamedTypeSymbol classDeclaration, string attributeValue)
        {
            var generateCodeAttribute = classDeclaration.GetAttributes().FirstOrDefault(attr =>
                attr.AttributeClass.ToDisplayString().Contains("Data.Attributes.GenerateCode") &&
                attr.ConstructorArguments.Length == 1 &&
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
