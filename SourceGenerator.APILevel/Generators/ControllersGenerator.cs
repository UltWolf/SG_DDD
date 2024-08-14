using Microsoft.CodeAnalysis;
using SourceGenerator.APILevel.Helpers;
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
            //System.Diagnostics.Debugger.Launch();
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
                            var additionalNamespaces = ReferenceHelper.GetAdditionalNamespaces(compilation);

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

        private string GenerateController(string namespaceName, string className, string baseController, AssemblyRefs assemblyRefs)
        {
            var baseControllerInheritance = string.IsNullOrWhiteSpace(baseController) ? "ControllerBase" : baseController.Split('.').Last();
            var usingBaseController = string.IsNullOrWhiteSpace(baseController) ? "" : $"using {string.Join(".", baseController.Split('.').Reverse().Skip(1).Reverse())};";

            var getAction = GenerateGetAction(className);
            var postAction = GeneratePostAction(className);
            var deleteAction = GenerateDeleteAction(className);
            var putAction = GeneratePutAction(className);
            var constructor = GenerateConstructor(className);
            return $@"
{usingBaseController} 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using {assemblyRefs.ApplicationName}.{className}.Dto;
using {assemblyRefs.ApplicationName}.{className}.Commands;
using {assemblyRefs.ApplicationName}.{className}.Queries;
using {assemblyRefs.DomainName}.{className}.Entity.Filters; 
namespace API.Controllers;

[Route(""api/[controller]"")]
[ApiController]
public class {className}Controller : {baseControllerInheritance}
{{
{constructor}
    {putAction} 
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
        private string GenerateConstructor(string className)
        {
            return $@"
                 private readonly ISender Mediator;
                 public {className}Controller(ISender mediator){{
                          Mediator = mediator;
                 }}
              ";
        }
        private string GeneratePutAction(string className)
        {
            return $@"
    [ProducesResponseType(200, Type = typeof(List<{className}Dto>))]
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] {className}Dto dto)
    {{
        return Ok(await Mediator.Send(new Update{className}Command
        {{
            {className} = dto
        }}));
    }}";
        }
        private string GenerateGetAction(string className)
        {
            return $@"
    [ProducesResponseType(200, Type = typeof(List<{className}Dto>))]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] {className}Filter filter)
    {{
        return Ok(await Mediator.Send(new Get{className}sQuery
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
        if(result.AsT0!=null){{
            return Ok(result);
        }}
        return BadRequest();
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
