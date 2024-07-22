using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Helper;

namespace SourceGenerator.DomainLevel.Generators
{
    [Generator]
    public class EntityConfigurationGenerator : ISourceGenerator
    {
        private Compilation compilation;
        private readonly List<ClassesToInsert> classesToInsert = new List<ClassesToInsert>();
        public void Execute(GeneratorExecutionContext context)
        {
            compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;

            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var assemblyName = compilation.AssemblyName;

                if (assemblyName == null || !assemblyName.Contains(".Domain"))
                {
                    continue;
                }
                var domainReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Domain"));
                if (domainReference != null)
                {
                    var domainAssembly = compilation.GetAssemblyOrModuleSymbol(domainReference) as IAssemblySymbol;

                    if (domainAssembly != null)
                    {
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, "EFConfigurations").ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var @namespace = classDeclaration.ContainingNamespace.ToDisplayString();
                            var className = classDeclaration.Name;

                            var baseOutputDir = Path.Combine(className.Trim(), "Configurations");

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {

                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}Configuration.cs",
                                        Generated =  GenerateEntityConfigurationClass(@namespace, className),
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
                            FileHelper.WriteToFile(generatedClass.PathToOutput, generatedClass.ClassName, generatedClass.Generated);
                        }
                    }
                }
            }
        }

        private IEnumerable<ClassDeclarationSyntax> GetClassDeclarations(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        }

        private string GenerateEntityConfigurationClass(string namespaceName, string className)
        {
            var entityName = className.Replace("Configuration", string.Empty);
            return $@"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {namespaceName}.Constants;
using {namespaceName}.Entities;

namespace {namespaceName}.Configurations
{{
    public class {className} : IEntityTypeConfiguration<{entityName}>
    {{
        public void Configure(EntityTypeBuilder<{entityName}> builder)
        {{
            builder.ToTable({entityName}Constants.TABLE_NAME);
 
        }}
    }}
}}
";
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
