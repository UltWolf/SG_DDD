using Microsoft.CodeAnalysis;
using SourceGeneratorLib.Data;
#pragma warning disable RS1035
namespace SourceGeneratorLib.Generators
{
    [Generator]
    public class DTOGenerator : ISourceGenerator
    {
        private Compilation compilation;
        private readonly List<ClassesToInsert> classesToInsert = new List<ClassesToInsert>();

        public void Execute(GeneratorExecutionContext context)
        {
            compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;

            //SpinWait.SpinUntil(() => Debugger.IsAttached);
            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var assemblyName = compilation.AssemblyName;

                // Check if current assembly name contains ".Application"
                if (assemblyName == null || !assemblyName.Contains(".Application"))
                {
                    continue;
                }

                var domainReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Domain"));
                if (domainReference != null)
                {
                    // Assembly.Load(domainReference.Display);
                    var domainAssembly = compilation.GetAssemblyOrModuleSymbol(domainReference) as IAssemblySymbol;

                    if (domainAssembly != null)
                    {
                        var classDeclarations = GetClassDeclarations(domainAssembly.GlobalNamespace).ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var @namespace = classDeclaration.ContainingNamespace.ToDisplayString();
                            var className = classDeclaration.Name;

                            var baseOutputDir = Path.Combine(className.Trim(), "Dto");

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}DTO.cs",
                                        Generated = GenerateDTO(@namespace, className, classDeclaration),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Create{className}DTO.cs",
                                        Generated = GenerateCreateDTO(@namespace, className, classDeclaration),
                                        PathToOutput = baseOutputDir
                                    }
                                }
                            };

                            classesToInsert.Add(classToInsert);
                        }
                    }
                }

                if (classesToInsert.Count > 0)
                {
                    foreach (var classToInsert in classesToInsert)
                    {
                        foreach (var generatedClass in classToInsert.GeneratedClasses)
                        {
                            WriteToFile(generatedClass.PathToOutput, generatedClass.ClassName, generatedClass.Generated);
                        }
                    }
                }
            }
        }

        private IEnumerable<INamedTypeSymbol> GetClassDeclarations(INamespaceSymbol namespaceSymbol)
        {
            var classDeclarations = new List<INamedTypeSymbol>();

            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (member is INamespaceSymbol nestedNamespace)
                {
                    // Recursively get class declarations from nested namespaces
                    classDeclarations.AddRange(GetClassDeclarations(nestedNamespace));
                }
                else if (member is INamedTypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Class)
                {
                    if (member.Name != "<Module>" && HasGenerateCodeAttribute(typeSymbol, "DTO"))

                    {
                        //var properties = GetProperties(typeSymbol);
                        classDeclarations.Add(typeSymbol);
                    }
                }
            }

            return classDeclarations;
        }
        private bool HasGenerateCodeAttribute(INamedTypeSymbol classDeclaration, string attributeValue)
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



        private string GenerateDTO(string namespaceName, string className, INamedTypeSymbol classSymbol)
        {
            var properties = string.Join(Environment.NewLine, classSymbol.GetMembers().OfType<IPropertySymbol>()
                .Select(p => $"        public {p.Type} {p.Name} {{ get; set; }}"));

            return $@"
namespace {namespaceName}.DTOs
{{
    public record {className}DTO
    {{
{properties}
    }}
}}";
        }

        private string GenerateCreateDTO(string namespaceName, string className, INamedTypeSymbol classSymbol)
        {
            var properties = string.Join(Environment.NewLine, classSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                .Select(p => $"        public {p.Type} {p.Name} {{ get; set; }}"));

            return $@"
namespace {namespaceName}.Dto
{{
    public record Create{className}Dto
    {{
{properties}
    }}
}}";
        }

        private void WriteToFile(string directory, string fileName, string content)
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

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }


}
