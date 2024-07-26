namespace SourceGenerator.DomainLevel.Generators.Repository
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using SourceGenerator.Common.Data;
    using SourceGenerator.Common.Helper;
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
            var domainClassDeclarations = new List<INamedTypeSymbol>();
            var baseFilterClass = (INamedTypeSymbol)null;
            var baseFilterNamespace = string.Empty;
            //System.Diagnostics.Debugger.Launch();

            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = syntaxTree.GetRoot();

                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Select(cds => semanticModel.GetDeclaredSymbol(cds))
                    .OfType<INamedTypeSymbol>()
                     .Where(cds => HasGenerateCodeAttribute(cds))
                    .ToList();

                if (classDeclarations.Count == 0)
                {
                    Console.WriteLine("No class declarations found with 'GenerateCode' attribute in the current syntax tree.");
                }
                else
                {
                    foreach (var classDecl in classDeclarations)
                    {
                        Console.WriteLine($"Found class with 'GenerateCode' attribute: {classDecl.Name}");
                    }
                }

                domainClassDeclarations.AddRange(classDeclarations);

                if (baseFilterClass == null)
                {
                    (baseFilterClass, baseFilterNamespace) = GetBaseFilterClassFromCompilation(semanticModel.Compilation);
                }
            }


            foreach (var classDeclaration in domainClassDeclarations)
            {
                var @namespace = classDeclaration.ContainingNamespace.ToDisplayString();
                var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
                var baseOutputDir = className.Trim();

                var classToInsert = new ClassesToInsert
                {
                    ClassName = className,
                    GeneratedClasses = new List<GeneratedClass>
                    {

                        new GeneratedClass
                        {
                            ClassName = $"{className}Repository.cs",
                            Generated = GenerateRepository(@namespace, baseFilterClass, classDeclaration),
                            PathToOutput = Path.Combine(baseOutputDir,"Repository")
                        },
                        new GeneratedClass
                        {
                            ClassName = $"{className}Id.cs",
                            Generated = GenerateValueTypeId(@namespace, classDeclaration),
                            PathToOutput = Path.Combine(baseOutputDir,"ValueTypes")
                        },
                           new GeneratedClass
                        {
                            ClassName = $"{className}Constants.cs",
                            Generated = GenerateConstantsClass(@namespace, className),
                            PathToOutput = Path.Combine(baseOutputDir,"Constants")
                        },
                         new GeneratedClass
                        {
                            ClassName = $"{className}Configuration.cs",
                            Generated = GenerateEntityConfigurationClass(@namespace,  className,classDeclaration),
                            PathToOutput = Path.Combine(baseOutputDir,"Configuration")
                        },
                        new GeneratedClass
                        {
                            ClassName = $"{className}Filter.cs",
                            Generated = GenerateFilter(@namespace, baseFilterClass, baseFilterNamespace,  ClassDeclarationHelper.GetNameForClass(classDeclaration)),
                            PathToOutput = Path.Combine(baseOutputDir,"Filter")
                        }
                    }
                };

                classesToInsert.Add(classToInsert);
            }

            if (classesToInsert.Count > 0)
            {
                CodeGenerationHelper.WriteGeneratedClasses(classesToInsert);
            }
        }
        private static bool HasGenerateCodeAttribute(INamedTypeSymbol classSymbol)
        {
            foreach (var attribute in classSymbol.GetAttributes())
            {
                if (attribute.AttributeClass != null && attribute.AttributeClass.ToDisplayString().Contains("GenerateRepositoryAttribute"))
                {
                    return true;
                }
            }
            return false;
        }
        private string GenerateConstantsClass(string namespaceName, string className)
        {
            return $@" 
namespace {namespaceName}.Constants
{{
    public class {className}Constants  
    {{
        public static string TABLE_NAME => ""{className}"";
    }}
}}
";
        }
        private string GenerateEntityConfigurationClass(string namespaceName, string className, INamedTypeSymbol classDeclaration)
        {

            return $@"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {namespaceName}.Constants; 

namespace {namespaceName}.Configurations
{{
    public class {className}Configuration : IEntityTypeConfiguration<{classDeclaration.Name}>
    {{
        public void Configure(EntityTypeBuilder<{classDeclaration.Name}> builder)
        {{
            builder.ToTable({className}Constants.TABLE_NAME);
 
        }}
    }}
}}
";
        }
        private INamedTypeSymbol GetContextTypeFromAttribute(INamedTypeSymbol classSymbol, int indexOfArgument)
        {
            foreach (var attribute in classSymbol.GetAttributes())
            {
                if (attribute.AttributeClass != null && attribute.AttributeClass.ToDisplayString().Contains("GenerateRepositoryAttribute"))
                {
                    if (attribute.ConstructorArguments.Length > 0)
                    {
                        var argumentValue = attribute.ConstructorArguments[indexOfArgument].Value;
                        if (argumentValue is INamedTypeSymbol contextType)
                        {
                            return contextType;
                        }
                    }
                }
            }
            return null;
        }
        private string GenerateRepository(string namespaceName, INamedTypeSymbol baseFilterClass, INamedTypeSymbol classDeclaration)
        {
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
            var contextType = GetContextTypeFromAttribute(classDeclaration, 0);
            var contextNamespace = contextType?.ContainingNamespace.ToDisplayString();
            var repositoryType = GetContextTypeFromAttribute(classDeclaration, 1);
            var repositoryNamespace = repositoryType?.ContainingNamespace.ToDisplayString();
            var strongType = GetContextTypeFromAttribute(classDeclaration, 2);
            var strongNameId = strongType != null ? $"{className}Id" : "";
            var contextUsing = !string.IsNullOrEmpty(contextNamespace) ? $"using {contextNamespace};" : string.Empty;

            var filterName = baseFilterClass != null ? $",{className}Filter" : string.Empty;
            var filterUsing = baseFilterClass != null ? $"using {namespaceName}.Filters" : string.Empty;
            var repositoryInterface = $@"
{contextUsing}
{filterUsing}
{$"using {namespaceName}.ValueTypes;"}
{$"using {repositoryNamespace};"}
namespace {namespaceName}.Repositories
{{
    public interface I{className}Repository : I{repositoryType.Name}<{classDeclaration.Name}, {strongNameId}{filterName}>
    {{ 
    }} 
";
            var repositoryImpl = $@"
 
    public class {className}Repository : {repositoryType.Name}<{classDeclaration.Name},{strongNameId},{contextType.Name}{filterName}>, I{className}Repository
    {{
        public {className}Repository({contextType.Name} context) : base(context)
        {{
        }}
    }}
}}
";
            return repositoryInterface + repositoryImpl;
        }



        private string GenerateValueTypeId(string namespaceName, INamedTypeSymbol classDeclaration)
        {
            var strongType = GetContextTypeFromAttribute(classDeclaration, 2);
            var strongConvertor = GetContextTypeFromAttribute(classDeclaration, 3);
            if (strongType == null || strongConvertor == null)
            {
                return "";
            }
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
            var strongTypeNamespace = $"using  {strongType.ContainingNamespace.ToDisplayString()};";
            var strongConvertorNamespace = $"using  {strongConvertor.ContainingNamespace.ToDisplayString()};";
            var strongNameId = strongType != null ? $"{className}Id" : "";
            return $@"
using System;
using System.ComponentModel;
{strongTypeNamespace}
{strongConvertorNamespace}
namespace {namespaceName}.ValueTypes
{{
    [TypeConverter(typeof({strongConvertor.Name}<{strongNameId}>))]
    public record {strongNameId}(Guid Value) : {strongType.Name};
}}
";
        }
        private (INamedTypeSymbol baseFilterClass, string namespaceName) GetBaseFilterClassFromCompilation(Compilation compilation)
        {
            var baseFilterClass = compilation.GlobalNamespace.GetTypeMembers().FirstOrDefault(t => t.Name == "BaseFilter");
            if (baseFilterClass != null)
            {
                return (baseFilterClass, baseFilterClass.ContainingNamespace.ToDisplayString());
            }
            return (null, null);
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
