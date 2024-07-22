
using Microsoft.CodeAnalysis;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Helper;
#pragma warning disable RS1035

namespace SourceGenerator.DomainLevel.Generators
{

    [Generator]
    public class DomainConstantsGenerator : ISourceGenerator
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
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, "TableConstants").ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var @namespace = classDeclaration.ContainingNamespace.ToDisplayString();
                            var className = classDeclaration.Name;

                            var baseOutputDir = Path.Combine(className.Trim(), "Constants");

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {

                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}Constants.cs",
                                        Generated =  GenerateConstantsClass(@namespace, className),
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

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}


