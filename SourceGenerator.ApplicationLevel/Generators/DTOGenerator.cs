using Microsoft.CodeAnalysis;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Helper;

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

            // System.Threading.SpinWait.SpinUntil(() => System.Diagnostics.Debugger.IsAttached);
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
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, "DTO").ToList();

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
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}MappingProfile.cs",
                                        Generated = GenerateMappingProfile(@namespace, className, classDeclaration),
                                        PathToOutput = Path.Combine(className.Trim(), "MappingProfile")
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
                            FileHelper.WriteToFile(generatedClass.PathToOutput, generatedClass.ClassName, generatedClass.Generated);
                        }
                    }
                }
            }
        }
        private string GenerateMappingProfile(string namespaceName, string className, INamedTypeSymbol classSymbol)
        {
            return $@"
using AutoMapper;
using {namespaceName}.Domains.{className};

namespace {namespaceName}.Application.{className}.DTO.MappingConfiguration;

public class {className}MappingProfile : Profile
{{
    public {className}MappingProfile()
    {{
        CreateMap<{className}, {className}Dto>();
        CreateMap<Create{className}Dto, {className}>();
    }}
}}";
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



        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }


}
