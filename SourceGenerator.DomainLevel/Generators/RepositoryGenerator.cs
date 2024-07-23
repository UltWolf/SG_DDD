
namespace SourceGenerator.DomainLevel.Generators
{
    using global::SourceGenerator.Common.Data;
    using global::SourceGenerator.Common.Helper;
    using Microsoft.CodeAnalysis;
    using SourceGenerator.Common.Data.Constants;
    using System.Collections.Generic;
    using System.Linq;

    [Generator]
    public class RepositoryGenerator : ISourceGenerator
    {
        private Compilation compilation;
        private readonly List<ClassesToInsert> classesToInsert = new List<ClassesToInsert>();

        public void Execute(GeneratorExecutionContext context)
        {
            compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;
            System.Diagnostics.Debugger.Launch();
            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var assemblyName = compilation.AssemblyName;

                if (assemblyName == null || !assemblyName.Contains(".Domain"))
                {
                    continue;
                }

                var domainReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Domain") && !r.Display.Contains("SourceGenerator.DomainLevel"));
                if (domainReference != null)
                {
                    var domainAssembly = compilation.GetAssemblyOrModuleSymbol(domainReference) as IAssemblySymbol;

                    if (domainAssembly != null)
                    {
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, LayerGenerate.EFRepositoryLayer).ToList();
                        var (baseFilterClass, baseFilterNamespace) = CodeGenerationHelper.GetBaseFilterClass(domainAssembly);


                        foreach (var classDeclaration in classDeclarations)
                        {
                            var @namespace = classDeclaration.ContainingNamespace.ToDisplayString();
                            var className = classDeclaration.Name;

                            var baseOutputDir = className.Trim();

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"I{className}Repository.cs",
                                        Generated = GenerateRepositoryInterface(@namespace, classDeclaration),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}Repository.cs",
                                        Generated = GenerateRepositoryImplementation(@namespace, classDeclaration),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}Id.cs",
                                        Generated = GenerateValueTypeId(@namespace, className),
                                        PathToOutput = baseOutputDir
                                    },
                                       new GeneratedClass
                                    {
                                        ClassName = $"{className}Filter.cs",
                                        Generated = GenerateFilter(@namespace, baseFilterClass, baseFilterNamespace, className),
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
                    CodeGenerationHelper.WriteGeneratedClasses(classesToInsert);
                    break;
                }
            }
        }

        private string GenerateRepositoryInterface(string namespaceName, INamedTypeSymbol classDeclaration)
        {
            var className = classDeclaration.Name;
            return $@"
namespace {namespaceName}.Repositories
{{
    public interface I{className}Repository : IBaseRepository<{className}, {className}Id, {className}Filter>
    {{
        Task<List<{className}>> GetAsync({className}Filter? filter, CancellationToken token);
    }}
}}
";
        }

        private string GenerateRepositoryImplementation(string namespaceName, INamedTypeSymbol classDeclaration)
        {
            var className = classDeclaration.Name;
            return $@"
namespace {namespaceName}.Repositories
{{
    public class {className}Repository : BaseRepository<{className}, {className}Id, ApplicationDbContext,  {className}Filter>, I{className}Repository
    {{
        public {className}Repository(ApplicationDbContext context) : base(context)
        {{
        }}
    }}
}}
";
        }

        private string GenerateValueTypeId(string namespaceName, string className)
        {
            return $@"
using System;
using System.ComponentModel;

namespace {namespaceName}.ValueTypes
{{
    [TypeConverter(typeof(StronglyTypedIdTypeConverter<{className}Id>))]
    public record {className}Id(Guid Value) : IStronglyTypedId;
}}
";
        }

        private string GenerateFilter(string namespaceName, INamedTypeSymbol baseFilterClass, string baseFilterNamespace, string className)
        {
            var baseFilter = baseFilterClass != null ? $"{baseFilterClass.Name}" : string.Empty;
            var baseFilterUsing = baseFilterClass != null ? $"using {baseFilterNamespace};" : string.Empty;
            var inheritance = baseFilterClass != null ? $" : {baseFilter}" : string.Empty;

            return $@"
{baseFilterUsing}
namespace {namespaceName}.Filters
{{
    public class {className}Filter{inheritance}
    {{
    }}
}}
";
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
