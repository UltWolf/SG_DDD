using Microsoft.CodeAnalysis;
using SourceGenerator.APILevel.Helpers;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Data.Constants;
using SourceGenerator.Common.Helper;
using System.Text;
#pragma warning disable RS1035
namespace SourceGenerator.APILevel.Generators
{
    [Generator]
    public class MinimalApiGenerator : ISourceGenerator
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
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, LayerGenerate.MinimalApiLayer).ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var idProperty = GetIdPropertyType(classDeclaration);

                            if (idProperty == null)
                            {
                                continue;
                            }
                            var @namespace = $"{assemblyName}";
                            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
                            var additionalNamespaces = ReferenceHelper.GetAdditionalNamespaces(compilation);

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"{className}Module.cs",
                                        Generated = GenerateCrudEndpoints(@namespace, className, additionalNamespaces, idProperty),
                                        PathToOutput = Path.Combine(className, "Module")
                                    }
                                }
                            };

                            classesToInsert.Add(classToInsert);
                        }
                    }

                    if (classesToInsert.Count > 0)
                    {
                        CodeGenerationHelper.WriteGeneratedClasses(classesToInsert);
                        var endpointsProgram = GetEndpointsModule(classesToInsert);

                        var programNewCs = ClassDeclarationHelper.GetModifiedRegion(endpointsProgram, "minimalAPI", root, context, assemblyName);
                        CodeGenerationHelper.WriteGeneratedClasses(
                            new List<ClassesToInsert>(){
                                new ClassesToInsert
                        {
                            GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"Program.cs",
                                        Generated = programNewCs,
                                        PathToOutput = ""
                                    }
                                }
                        } }, true);
                    }
                }
            }
        }
        private string GetEndpointsModule(List<ClassesToInsert> classesToInserts)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var c in classesToInserts.GroupBy(f => f.ClassName))
            {
                stringBuilder.Append($"app.Add{c.Key}EndpointsModule();");
            }
            return stringBuilder.ToString();
        }
        private string GenerateCrudEndpoints(string @namespace, string className, AssemblyRefs assemblyRefs, IPropertySymbol idProperty)
        {
            var lowerClassName = char.ToLower(className[0]) + className.Substring(1);

            var useMediator = IsMediatorAvailable();

            var getAction = GenerateGetAction(className, lowerClassName, assemblyRefs, useMediator, idProperty);
            var postAction = GeneratePostAction(className, lowerClassName, assemblyRefs, useMediator);
            var deleteAction = GenerateDeleteAction(className, lowerClassName, assemblyRefs, useMediator, idProperty);
            var putAction = GeneratePutAction(className, lowerClassName, assemblyRefs, useMediator);

            return $@"
using Microsoft.AspNetCore.Mvc;
using {(useMediator ? "MediatR;" : $"{assemblyRefs.ApplicationName}.Services;")}
using {assemblyRefs.ApplicationName}.{className}.Dto;
using {(useMediator ? $"{assemblyRefs.ApplicationName}.{className}.Commands;" : string.Empty)}
using {(useMediator ? $"{assemblyRefs.ApplicationName}.{className}.Queries;" : string.Empty)}
using {assemblyRefs.DomainName}.{className}.Entity.Filters;
using Swashbuckle.AspNetCore.Annotations;

namespace {@namespace}.Modules
{{
    public static class {className}Module
    {{
        public static WebApplication Add{className}EndpointsModule(this WebApplication app)
        {{
            {postAction}
            {getAction}
            {putAction}
            {deleteAction}
            
            return app;
        }}
    }}
}}
";
        }

        private bool IsMediatorAvailable()
        {
            return compilation.ReferencedAssemblyNames.Any(r => r.Name.Equals("MediatR", StringComparison.OrdinalIgnoreCase));
        }

        private IPropertySymbol GetIdPropertyType(INamedTypeSymbol classDeclaration)
        {
            var idProperty = classDeclaration.GetMembers().OfType<IPropertySymbol>().FirstOrDefault(prop => prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            if (idProperty == null)
            {
                return null;
            }

            if (idProperty.Type.IsValueType || idProperty.Type.SpecialType != SpecialType.None)
            {
                return idProperty;
            }

            var valueProperty = idProperty.Type.GetMembers().OfType<IPropertySymbol>().FirstOrDefault(prop => prop.Name.Equals("Value", StringComparison.OrdinalIgnoreCase));

            return valueProperty ?? idProperty;
        }
        private string GenerateGetAction(string className, string lowerClassName, AssemblyRefs assemblyRefs, bool useMediator, IPropertySymbol idType)
        {
            if (useMediator)
            {
                return $@"
    app.MapGet(""/{lowerClassName}s/{{id}}"", async ({CodeGenerationHelper.GetTypeOfId(idType)} id, IMediator mediator) =>
    {{
        var query = new Get{className}Query(){{{className}Id =  id}};
        var result = await mediator.Send(query);
        return result.IsT0 ? Results.Ok(result.AsT0) : Results.NotFound(new {{ Message = $""{className} with ID {{id}} not found."" }});
    }})
    .WithName(""Get{className}ById"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Get {className} by ID"", description: ""Fetches a {className} by their ID""))
    .Produces<{className}Dto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
";
            }
            else
            {
                return $@"
    app.MapGet(""/{lowerClassName}s/{{id}}"", async ({CodeGenerationHelper.GetTypeOfId(idType)} id, {className}Service _service) =>
    {{
        var result = await _service.Get{className}ByIdAsync(id);
        return result.IsT0() ? Results.Ok(result.AsT0()) : Results.NotFound(new {{ Message = $""{className} with ID {{id}} not found."" }});
    }})
    .WithName(""Get{className}ById"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Get {className} by ID"", description: ""Fetches a {className} by their ID""))
    .Produces<{className}Dto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
";
            }
        }

        private string GeneratePostAction(string className, string lowerClassName, AssemblyRefs assemblyRefs, bool useMediator)
        {
            if (useMediator)
            {
                return $@"
    app.MapPost(""/{lowerClassName}s"", async (Create{className}Dto {lowerClassName}, IMediator mediator) =>
    {{
        var command = new Create{className}Command(){{{className} = {lowerClassName}}};
        var result = await mediator.Send(command);
          return result.IsT0 ?  Results.Ok(new {{ Message = ""{className} created successfully."", {className} = result.AsT0 }}): Results.BadRequest(result.AsT0);
    }})
    .WithName(""Create{className}"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Create {className}"", description: ""Creates a new {className}""))
    .Produces<{className}Dto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);
";
            }
            else
            {
                return $@"
    app.MapPost(""/{lowerClassName}s"", async (Create{className}Dto {lowerClassName}, {className}Service _service) =>
    {{
        var result = await _service.Create{className}Async({lowerClassName});
       return result.IsT0 ?  Results.Ok(new {{ Message = ""{className} created successfully."", {className} = result.AsT0 }}): Results.BadRequest(result.AsT0));
    }})
    .WithName(""Create{className}"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Create {className}"", description: ""Creates a new {className}""))
    .Produces<{className}Dto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);
";
            }
        }

        private string GeneratePutAction(string className, string lowerClassName, AssemblyRefs assemblyRefs, bool useMediator)
        {
            if (useMediator)
            {
                return $@"
    app.MapPut(""/{lowerClassName}s"", async ({className}Dto {lowerClassName}, IMediator mediator) =>
    {{
        var command = new Update{className}Command(){{{className}= {lowerClassName} }};
        var result = await mediator.Send(command);
        return result.IsT0 ? Results.Ok(new {{ Message = ""{className} updated successfully."", {className} = result.AsT0 }}) : Results.NotFound(new {{ Message = $""{className}   not found."" }});
    }})
    .WithName(""Update{className}"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Update {className}"", description: ""Updates an existing {className}""))
    .Produces<{className}Dto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
";
            }
            else
            {
                return $@"
    app.MapPut(""/{lowerClassName}s"", async ( {className}Dto {lowerClassName}, {className}Service _service) =>
    {{
        var result = await _service.Update{className}Async(  {lowerClassName});
        return result != null ? Results.Ok(new {{ Message = ""{className} updated successfully."", {className} = result }}) : Results.NotFound(new {{ Message = $""{className}  not found."" }});
    }})
    .WithName(""Update{className}"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Update {className}"", description: ""Updates an existing {className}""))
    .Produces<{className}Dto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
";
            }
        }

        private string GenerateDeleteAction(string className, string lowerClassName, AssemblyRefs assemblyRefs, bool useMediator, IPropertySymbol idType)
        {

            if (useMediator)
            {
                return $@"
    app.MapDelete(""/{lowerClassName}s/{{id}}"", async ({CodeGenerationHelper.GetTypeOfId(idType)} id, IMediator mediator) =>
    {{
        var command = new Delete{className}Command(){{{className}Id = id}};
          await mediator.Send(command);
                return Results.Ok(new {{ Message = ""User deleted successfully."" }});
    }})
    .WithName(""Delete{className}"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Delete {className}"", description: ""Deletes an existing {className}""))
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
";
            }
            else
            {
                return $@"
    app.MapDelete(""/{lowerClassName}s/{{id}}"", async ({CodeGenerationHelper.GetTypeOfId(idType)} id, {className}Service _service) =>
    {{
        var result = await _service.Delete{className}Async(id);
        return result != null ? Results.Ok(new {{ Message = ""{className} deleted successfully."" }}) : Results.NotFound(new {{ Message = $""{className}   not found."" }});
    }})
    .WithName(""Delete{className}"")
    .WithMetadata(new SwaggerOperationAttribute(summary: ""Delete {className}"", description: ""Deletes an existing {className}""))
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
";
            }
        }
    }
}
