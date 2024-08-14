using Microsoft.CodeAnalysis;
using SourceGenerator.APILevel.Helpers;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Data.Attributes;
using SourceGenerator.Common.Data.Constants;
using SourceGenerator.Common.Helper;
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid uninstantiated internal classes", Justification = "Source Generator")]
namespace SourceGenerator.ApplicationLevel.Generators
{
#pragma warning disable RS1035
    [Generator]
    public class FastEndpointGenerator : ISourceGenerator
    {
        private Compilation compilation;
        private readonly List<ClassesToInsert> classesToInsert = new List<ClassesToInsert>();
        private bool hasOneOfReference;
        private bool hasMediatRReference;

        public void Execute(GeneratorExecutionContext context)
        {
            compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;
            hasOneOfReference = compilation.References.Any(r => r.Display.Contains("OneOf"));
            hasMediatRReference = compilation.References.Any(r => r.Display.Contains("MediatR"));

            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var assemblyName = compilation.AssemblyName;

                if (assemblyName == null || !assemblyName.Contains(".API"))
                {
                    continue;
                }

                var domainReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Domain"));
                if (domainReference != null)
                {
                    var domainAssembly = compilation.GetAssemblyOrModuleSymbol(domainReference) as IAssemblySymbol;

                    if (domainAssembly != null)
                    {
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, LayerGenerate.MediatRCommandLayer).ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var attributeData = classDeclaration.GetAttributes()
                                .FirstOrDefault(ad => ad.AttributeClass.Name == nameof(GenerateCodeAttribute) && ad.ConstructorArguments[0].Value.ToString().Contains(LayerGenerate.FastEndpointLayer));

                            if (attributeData == null)
                            {
                                continue;
                            }

                            var attributeArguments = attributeData.ConstructorArguments;
                            var additionalType = attributeArguments.Length == 2 ? attributeArguments[1].Value as INamedTypeSymbol : null;

                            var additionalNamespaces = ReferenceHelper.GetAdditionalNamespaces(compilation);
                            var @namespace = $"{assemblyName}.{ClassDeclarationHelper.GetNameForClass(classDeclaration)}";
                            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);

                            var baseOutputDir = Path.Combine(className.Trim(), "Endpoints");

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"Create{className}Endpoint.cs",
                                        Generated = GenerateCreateEndpoint(@namespace, classDeclaration,additionalNamespaces),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Delete{className}Endpoint.cs",
                                        Generated = GenerateDeleteEndpoint(@namespace, classDeclaration,additionalNamespaces),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Update{className}Endpoint.cs",
                                        Generated = GenerateUpdateEndpoint(@namespace, classDeclaration,additionalNamespaces),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Get{className}Endpoint.cs",
                                        Generated = GenerateGetEndpoint(@namespace, classDeclaration,additionalNamespaces),
                                        PathToOutput = baseOutputDir.Replace("Endpoints", "Queries")
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Get{className}sEndpoint.cs",
                                        Generated = GenerateGetAllEndpoint(@namespace, classDeclaration,additionalNamespaces),
                                        PathToOutput = baseOutputDir.Replace("Endpoints", "Queries")
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

        private string GenerateCreateEndpoint(string namespaceName, INamedTypeSymbol classDeclaration, AssemblyRefs? assemblyRefs)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();

            return $@"
using FastEndpoints;
using AutoMapper;
using MediatR; 
using {assemblyRefs.ApplicationName}.{className}.Dto;
using {assemblyRefs.ApplicationName}.{className}.Commands;
using {assemblyRefs.ApplicationName}.{className}.Queries;
using {assemblyRefs.DomainName}.{className}.Entity.Filters; 

namespace {namespaceName}.Endpoints
{{
    public class Create{className}Endpoint : Endpoint<Create{className}Dto, {className}Dto>
    {{
        private readonly AutoMapper.IMapper _mapper; 
        private readonly IMediator _mediator;

        public Create{className}Endpoint( AutoMapper.IMapper mapper, IMediator mediator)
        {{
            _mapper = mapper; 
            _mediator = mediator;
        }}

        public override void Configure()
        {{
            Post(""/{className.ToLower()}"");
        }}
  public override async Task<Results<Ok<{className}Dto>, NotFound, ProblemDetails>> ExecuteAsync(
         Create{className}Dto req, CancellationToken ct)
        {{
            var result = await _mediator.Send(new Create{className}Command
            {{
                User = req
            }});
            if (result.AsT0 != null)
            {{
                return TypedResults.Ok(result.AsT0);
            }}
            else
            {{
                AddError(result.AsT1.Message);
                return new FastEndpoints.ProblemDetails(ValidationFailures);
            }}
        }}
       
    }}
}}
";
        }

        private string GenerateDeleteEndpoint(string namespaceName, INamedTypeSymbol classDeclaration, AssemblyRefs? assemblyRefs)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration); ;
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            return $@"
using FastEndpoints;
using AutoMapper;
using MediatR;
using {entityNamespace};
using {assemblyRefs.ApplicationName}.{className}.Dto;
using {assemblyRefs.ApplicationName}.{className}.Commands;
using {assemblyRefs.ApplicationName}.{className}.Queries;
using {assemblyRefs.DomainName}.{className}.Entity.Filters; 

namespace {namespaceName}.Endpoints
{{
    public class Delete{className}Endpoint : Endpoint<Delete{className}Request>
    {{ 
        private readonly IMediator _mediator;

        public Delete{className}Endpoint(  IMediator mediator)
        {{ 
            _mediator = mediator;
        }}

        public override void Configure()
        {{
            Delete(""/{className.ToLower()}/{{{className}Id}}"");
        }}

        public override async Task HandleAsync(Delete{className}Request request, CancellationToken cancellationToken)
        {{ 
            await _mediator.Send(new Delete{className}Command() {{{className}Id = request.Id.Value}});
            await SendOkAsync();
        }}
    }}
 public class Delete{className}Request
    {{
        public {className}Id Id {{ get; set; }}
    }}
}}
";
        }

        private string GenerateUpdateEndpoint(string namespaceName, INamedTypeSymbol classDeclaration, AssemblyRefs? assemblyRefs)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
            //var returnType = hasOneOfReference ? $"OneOf<{className}Dto, {additionalTypeName}>" : $"{className}Dto";
            //var returnNamespace = hasOneOfReference ? $"using OneOf;{Environment.NewLine}{additionalTypeNamespace}" : string.Empty;
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();

            return $@"
using FastEndpoints;
using AutoMapper;
using MediatR;  
using {assemblyRefs.ApplicationName}.{className}.Dto;
using {assemblyRefs.ApplicationName}.{className}.Commands;
using {assemblyRefs.ApplicationName}.{className}.Queries;
using {assemblyRefs.DomainName}.{className}.Entity.Filters; 

namespace {namespaceName}.Endpoints
{{
    public class Update{className}Endpoint : Endpoint<{className}Dto, {className}Dto>
    {{
        private readonly AutoMapper.IMapper _mapper; 
        private readonly IMediator _mediator;

        public Update{className}Endpoint( AutoMapper.IMapper mapper, IMediator mediator)
        {{
            _mapper = mapper; 
            _mediator = mediator;
        }}

        public override void Configure()
        {{
            Put(""/{className.ToLower()}"");
        }}

        public override async Task HandleAsync({className}Dto request, CancellationToken cancellationToken)
        {{
             var entity = await _mediator.Send(new Update{className}Command{{{className}= request}}); 
            await SendAsync(_mapper.Map<{className}Dto>(entity));
        }}
    }}
}}
";
        }

        private string GenerateGetEndpoint(string namespaceName, INamedTypeSymbol classDeclaration, AssemblyRefs? assemblyRefs)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration); ;
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();

            return $@"
using FastEndpoints;
using AutoMapper;
using MediatR; 
using {assemblyRefs.ApplicationName}.{className}.Dto;
using {assemblyRefs.ApplicationName}.{className}.Commands;
using {assemblyRefs.ApplicationName}.{className}.Queries;
using {assemblyRefs.DomainName}.{className}.Entity.Filters; 
namespace {namespaceName}.Queries
{{
    public class Get{className}Endpoint : Endpoint<Get{className}Request, {className}Dto>
    {{
        private readonly IMapper _mapper;
        private readonly I{className}Repository _repository;

        public Get{className}Endpoint(I{className}Repository repository, IMapper mapper)
        {{
            _repository = repository;
            _mapper = mapper;
        }}

        public override void Configure()
        {{
            Get(""/{className.ToLower()}/{{{className}Id}}"");
        }}

        public override async Task HandleAsync(Get{className}Request request, CancellationToken cancellationToken)
        {{
            var entity = await _repository.GetAsync(new {classDeclaration.Name}() {{ Id = new {className}Id(request.{className}Id) }}, cancellationToken);
            await SendAsync(_mapper.Map<{className}Dto>(entity));
        }}
    }}
}}
";
        }

        private string GenerateGetAllEndpoint(string namespaceName, INamedTypeSymbol classDeclaration, AssemblyRefs? assemblyRefs)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();

            return $@"
using FastEndpoints;
using AutoMapper;
using MediatR; 
using {assemblyRefs.ApplicationName}.{className}.Dto;
using {assemblyRefs.ApplicationName}.{className}.Commands;
using {assemblyRefs.ApplicationName}.{className}.Queries;
using {assemblyRefs.DomainName}.{className}.Entity.Filters; 

namespace {namespaceName}.Queries
{{
    public class Get{className}sEndpoint : EndpointWithoutRequest<List<{className}Dto>>
    {{
        private readonly IMapper _mapper;
        private readonly I{className}Repository _repository;

        public Get{className}sEndpoint(I{className}Repository repository, IMapper mapper)
        {{
            _repository = repository;
            _mapper = mapper;
        }}

        public override void Configure()
        {{
            Get(""/{className.ToLower()}s"");
        }}

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {{
            var entities = await _repository.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<List<{className}Dto>>(entities);
            await SendAsync(dtos);
        }}
    }}
}}
";
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }

}


