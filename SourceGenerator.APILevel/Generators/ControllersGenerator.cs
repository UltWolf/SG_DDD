using Microsoft.CodeAnalysis;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Data.Constants;
using SourceGenerator.Common.Helper;
#pragma warning disable RS1035
namespace SourceGenerator.APILevel.Generators
{
    [Generator]
    public class ControllerGenerator : ISourceGenerator
    {
        private Compilation compilation;
        private readonly List<ClassesToInsert> classesToInsert = new List<ClassesToInsert>();

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for now
        }

        public void Execute(GeneratorExecutionContext context)
        {
            compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;

            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var assemblyName = compilation.AssemblyName;

                if (assemblyName == null || !assemblyName.Contains(".API"))
                {
                    continue;
                }

                var domainReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Domain") && !r.Display.Contains(".DomainLevel"));
                if (domainReference != null)
                {
                    var domainAssembly = compilation.GetAssemblyOrModuleSymbol(domainReference) as IAssemblySymbol;

                    if (domainAssembly != null)
                    {
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, LayerGenerate.ControllerLayer).ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var @namespace = $"{assemblyName}.{ClassDeclarationHelper.GetNameForClass(classDeclaration)}";
                            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
                            var baseController = GetBaseController(classDeclaration);
                            var additionalNamespaces = GetAdditionalNamespaces(compilation);

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}Controller.cs",
                                        Generated = GenerateController(@namespace, className, baseController, additionalNamespaces),
                                        PathToOutput = "Controllers"
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

        private string GenerateController(string namespaceName, string className, string baseController, List<string> additionalNamespaces)
        {
            var baseControllerInheritance = string.IsNullOrWhiteSpace(baseController) ? "ApiBaseController" : baseController.Split('.').Last();
            var usingBaseController = string.IsNullOrWhiteSpace(baseController) ? "" : $"using {string.Join(".", baseController.Split('.').Reverse().Skip(1).Reverse())};";
            var additionalUsings = string.Join(Environment.NewLine, additionalNamespaces.Select(ns => $"using {ns};"));
            var getAction = GenerateGetAction(className);
            var postAction = GeneratePostAction(className);
            var deleteAction = GenerateDeleteAction(className);

            return $@"
{usingBaseController}
{additionalUsings}
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route(""api/[controller]"")]
[ApiController]
public class {className}Controller : {baseControllerInheritance}
{{
    {getAction}

    {postAction}

    {deleteAction}
}}";
        }

        private string GetBaseController(INamedTypeSymbol classSymbol)
        {
            var attribute = classSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass != null && attr.AttributeClass.Name == "GenerateCode" &&
                                         attr.ConstructorArguments.Length > 1 &&
                                         attr.ConstructorArguments[1].Value is INamedTypeSymbol);

            if (attribute == null)
            {
                return null;
            }

            var baseController = attribute.ConstructorArguments[1].Value as INamedTypeSymbol;
            return baseController?.ToDisplayString();
        }

        private List<string> GetAdditionalNamespaces(Compilation compilation)
        {
            var namespaces = new List<string>();

            var applicationReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Application"));
            if (applicationReference != null)
            {
                var applicationAssembly = compilation.GetAssemblyOrModuleSymbol(applicationReference) as IAssemblySymbol;
                if (applicationAssembly != null)
                {
                    namespaces.AddRange(GetNamespacesFromAssembly(applicationAssembly));
                }
            }

            var domainsReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Domains"));
            if (domainsReference != null)
            {
                var domainsAssembly = compilation.GetAssemblyOrModuleSymbol(domainsReference) as IAssemblySymbol;
                if (domainsAssembly != null)
                {
                    namespaces.AddRange(GetNamespacesFromAssembly(domainsAssembly));
                }
            }

            var infrastructureReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Infrastructure"));
            if (infrastructureReference != null)
            {
                var infrastructureAssembly = compilation.GetAssemblyOrModuleSymbol(infrastructureReference) as IAssemblySymbol;
                if (infrastructureAssembly != null)
                {
                    namespaces.AddRange(GetNamespacesFromAssembly(infrastructureAssembly));
                }
            }

            return namespaces;
        }

        private List<string> GetNamespacesFromAssembly(IAssemblySymbol assemblySymbol)
        {
            var namespaces = new List<string>();

            foreach (var module in assemblySymbol.Modules)
            {
                foreach (var ns in module.GlobalNamespace.GetNamespaceMembers())
                {
                    namespaces.Add(ns.ToDisplayString());
                }
            }

            return namespaces;
        }

        private string GenerateGetAction(string className)
        {
            return $@"
    [ProducesResponseType(200, Type = typeof(List<{className}Dto>))]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] {className}Filter filter)
    {{
        return ReturnOkResult<List<{className}Dto>>, TrainingError, TrainingErrorCodes>(await Mediator.Send(new Get{className}sQuery
        {{
            Filter = filter
        }}));
    }}";
        }

        private string GeneratePostAction(string className)
        {
            return $@"
    [ProducesResponseType(200, Type = typeof({className}Dto))]
    [HttpPost]
    public async Task<IActionResult> Post(Create{className}Dto create{className}Dto)
    {{
        var result = await Mediator.Send(new Create{className}Command
        {{
            {className} = create{className}Dto
        }});

        return ReturnOkResult<{className}Dto, TrainingError, TrainingErrorCodes>(result);
    }}";
        }

        private string GenerateDeleteAction(string className)
        {
            return $@"
    [ProducesResponseType(200)]
    [HttpDelete(""{{{className}Id}}"")]
    public async Task<IActionResult> Delete(Guid {className}Id)
    {{
        await Mediator.Send(new Delete{className}Command
        {{
            {className}Id = {className}Id
        }});
        return Ok();
    }}";
        }
    }
}
