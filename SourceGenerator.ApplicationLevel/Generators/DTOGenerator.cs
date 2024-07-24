using Microsoft.CodeAnalysis;
using SourceGenerator.ApplicationLevel.Constants;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Data.Constants;
using SourceGenerator.Common.Helper;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid uninstantiated internal classes", Justification = "Source Generator")]
namespace SourceGeneratorLib.Generators
{
    [Generator]
#pragma warning disable RS1035
    public class DTOGenerator : ISourceGenerator
    {
        private Compilation compilation;
        private readonly List<ClassesToInsert> classesToInsert = new List<ClassesToInsert>();

        public void Execute(GeneratorExecutionContext context)
        {
            compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;

            //System.Diagnostics.Debugger.Launch();
            //System.Threading.SpinWait.SpinUntil(() => System.Diagnostics.Debugger.IsAttached);
            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var assemblyName = compilation.AssemblyName;

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
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, LayerGenerate.DtoLayer).ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var @namespace = classDeclaration.ContainingNamespace.ToDisplayString().Replace("Domain", "");
                            var className = classDeclaration.Name;

                            var baseOutputDir = Path.Combine(className.Trim(), StringConstants.DTOEnding);

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}{StringConstants.DTOEnding}.cs",
                                        Generated = GenerateDTO(@namespace, className, classDeclaration),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Create{className}{StringConstants.DTOEnding}.cs",
                                        Generated = GenerateCreateDTO(@namespace, className, classDeclaration),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}MappingProfile.cs",
                                        Generated = GenerateMappingProfile(@namespace, className, classDeclaration),
                                        PathToOutput = Path.Combine(baseOutputDir, "MappingProfile")
                                    }
                                }
                            };

                            classesToInsert.Add(classToInsert);
                        }
                    }
                    if (classesToInsert.Count > 0)
                    {
                        CodeGenerationHelper.WriteGeneratedClasses(classesToInsert);
                        break;
                    }
                }
            }
        }
        private string GenerateMappingProfile(string namespaceName, string className, INamedTypeSymbol classSymbol)
        {
            return $@"
using AutoMapper;
using {namespaceName}.Domains.{className};

namespace {namespaceName}.Application.{className}.{StringConstants.DTOEnding}.MappingConfiguration;

public class {className}MappingProfile : Profile
{{
    public {className}MappingProfile()
    {{
        CreateMap<{className}, {className}{StringConstants.DTOEnding}>();
        CreateMap<Create{className}{StringConstants.DTOEnding}, {className}>();
    }}
}}";
        }






        private string GetDtoNamespace(string namespaceName)
        {
            return $"namespace {namespaceName}.Application.{StringConstants.DTOEnding}";
        }
        private string GenerateDTO(string namespaceName, string className, INamedTypeSymbol classSymbol)
        {
            var properties = string.Join(Environment.NewLine, classSymbol.GetMembers().OfType<IPropertySymbol>()
                .Select(p => $"        public {p.Type} {p.Name} {StringConstants.PropertiesAccessories}"));

            return $@"
{GetDtoNamespace(namespaceName)}
{{
    public record {className}{StringConstants.DTOEnding}
    {{
{properties}
    }}
}}";
        }

        private string GenerateCreateDTO(string namespaceName, string className, INamedTypeSymbol classSymbol)
        {
            var properties = string.Join(Environment.NewLine, classSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                .Select(p => $"        public {p.Type} {p.Name} {StringConstants.PropertiesAccessories}"));

            return $@"
{GetDtoNamespace(namespaceName)}
{{
    public record Create{className}{StringConstants.DTOEnding}
    {{
{properties}
    }}
}}";
        }



        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }


}
