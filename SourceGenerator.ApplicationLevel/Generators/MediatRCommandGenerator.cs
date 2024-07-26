using Microsoft.CodeAnalysis;
using SourceGenerator.Common.Data;
using SourceGenerator.Common.Data.Attributes;
using SourceGenerator.Common.Data.Constants;
using SourceGenerator.Common.Helper;
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid uninstantiated internal classes", Justification = "Source Generator")]
namespace SourceGenerator.ApplicationLevel.Generators
{

#pragma warning disable RS1035
    [Generator]
    public class MediatRCommandGenerator : ISourceGenerator
    {
        private Compilation compilation;
        private readonly List<ClassesToInsert> classesToInsert = new List<ClassesToInsert>();
        private bool hasOneOfReference;

        public void Execute(GeneratorExecutionContext context)
        {
            compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;
            hasOneOfReference = compilation.References.Any(r => r.Display.Contains("OneOf"));
            // System.Diagnostics.Debugger.Launch();
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
                    var domainAssembly = compilation.GetAssemblyOrModuleSymbol(domainReference) as IAssemblySymbol;

                    if (domainAssembly != null)
                    {
                        var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, LayerGenerate.MediatRCommandLayer).ToList();

                        foreach (var classDeclaration in classDeclarations)
                        {
                            var attributeData = classDeclaration.GetAttributes()
                                .FirstOrDefault(ad => ad.AttributeClass.Name == nameof(GenerateCodeAttribute) && ad.ConstructorArguments[0].Value.ToString().Contains(LayerGenerate.MediatRCommandLayer));

                            if (attributeData == null)
                            {
                                continue;
                            }

                            var attributeArguments = attributeData.ConstructorArguments;
                            var additionalType = attributeArguments.Length == 2 ? attributeArguments[1].Value as INamedTypeSymbol : null;

                            var additionalTypeName = additionalType?.ToDisplayString() ?? "object";
                            var additionalTypeNamespace = additionalType != null ? $"using {additionalType.ContainingNamespace.ToDisplayString()};" : string.Empty;

                            var @namespace = $"{assemblyName}.{ClassDeclarationHelper.GetNameForClass(classDeclaration)}";
                            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);

                            var baseOutputDir = Path.Combine(className.Trim(), "Commands");

                            var classToInsert = new ClassesToInsert
                            {
                                ClassName = className,
                                GeneratedClasses = new List<GeneratedClass>
                                    {
                                        new GeneratedClass
                                        {
                                            ClassName = $"Create{className}Command.cs",
                                            Generated = GenerateCreateCommand(@namespace, classDeclaration, additionalTypeName, additionalTypeNamespace),
                                            PathToOutput = baseOutputDir
                                        },
                                        new GeneratedClass
                                        {
                                            ClassName = $"Delete{className}Command.cs",
                                            Generated = GenerateDeleteCommand(@namespace, classDeclaration),
                                            PathToOutput = baseOutputDir
                                        },
                                        new GeneratedClass
                                        {
                                            ClassName = $"Update{className}Command.cs",
                                            Generated = GenerateUpdateCommand(@namespace, classDeclaration, additionalTypeName, additionalTypeNamespace),
                                            PathToOutput = baseOutputDir
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

        private string GenerateCreateCommand(string namespaceName, INamedTypeSymbol classDeclaration, string additionalTypeName, string additionalTypeNamespace)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
            var returnType = hasOneOfReference ? $"OneOf<{className}Dto, {additionalTypeName}>" : $"{className}Dto";
            var returnNamespace = hasOneOfReference ? $"using OneOf;{Environment.NewLine}{additionalTypeNamespace}" : string.Empty;
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();

            return $@"
using MediatR;
using AutoMapper;
{returnNamespace}
using {namespaceName}.Dto; 
using {entityNamespace};
using {domainNamespace}.Repositories; 
using {domainNamespace}.ValueTypes; 

namespace {namespaceName}.Commands
{{
    public class Create{className}Command : IRequest<{returnType}>
    {{
        public Create{className}Dto New{className} {{ get; init; }} = null!;
    }}

    public class Create{className}Handler : IRequestHandler<Create{className}Command, {returnType}>
    {{
        private readonly IMapper _mapper;
        private readonly I{className}Repository _repository;

        public Create{className}Handler(I{className}Repository repository, IMapper mapper)
        {{
            _mapper = mapper;
            _repository = repository;
        }}

        public async Task<{returnType}> Handle(Create{className}Command request, CancellationToken cancellationToken)
        {{
            var entity = _mapper.Map<{classDeclaration.Name}>(request.New{className});
            var newEntity = await _repository.AddAsync(entity, cancellationToken);
            return _mapper.Map<{className}Dto>(newEntity);
        }}
    }}
}}
";
        }

        private string GenerateDeleteCommand(string namespaceName, INamedTypeSymbol classDeclaration)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration); ;
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            return $@"
using MediatR;
using AutoMapper;
using {entityNamespace};
using {domainNamespace}.Repositories;
using {domainNamespace}.ValueTypes; 
namespace {namespaceName}.Commands
{{
    public class Delete{className}Command : IRequest
    {{
        public required Guid {className}Id {{ get; init; }}
    }}

    public class Delete{className}Handler : IRequestHandler<Delete{className}Command>
    {{
        private readonly I{className}Repository _repository;

        public Delete{className}Handler(I{className}Repository repository)
        {{
            _repository = repository;
        }}

        public Task Handle(Delete{className}Command request, CancellationToken cancellationToken)
        {{
            return _repository.DeleteAsync(new {classDeclaration.Name}() {{ Id = new {className}Id(request.{className}Id) }}, cancellationToken);
        }}
    }}
}}
";
        }

        private string GenerateUpdateCommand(string namespaceName, INamedTypeSymbol classDeclaration, string additionalTypeName, string additionalTypeNamespace)
        {
            var entityNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            var className = ClassDeclarationHelper.GetNameForClass(classDeclaration);
            var returnType = hasOneOfReference ? $"OneOf<{className}Dto, {additionalTypeName}>" : $"{className}Dto";
            var returnNamespace = hasOneOfReference ? $"using OneOf;{Environment.NewLine}{additionalTypeNamespace}" : string.Empty;
            var domainNamespace = classDeclaration.ContainingNamespace.ToDisplayString();
            return $@"
using MediatR;
using AutoMapper;
{returnNamespace}
using {namespaceName}.Dto; 
using {entityNamespace};
using {domainNamespace}.Repositories; 
using {domainNamespace}.ValueTypes; 
namespace {namespaceName}.Commands
{{
    public class Update{className}Command : IRequest<{returnType}>
    {{
        public {className}Dto Update{className} {{ get; init; }} = null!;
    }}

    public class Update{className}Handler : IRequestHandler<Update{className}Command, {returnType}>
    {{
        private readonly IMapper _mapper;
        private readonly I{className}Repository _repository; 

        public Update{className}Handler(I{className}Repository repository, IMapper mapper)
        {{
            _mapper = mapper;
            _repository = repository; 
        }}

        public async Task<{returnType}> Handle(Update{className}Command request, CancellationToken cancellationToken)
        {{
            var entity = _mapper.Map<{classDeclaration.Name}>(request.Update{className});
            var updatedEntity = await _repository.UpdateAsync(entity, cancellationToken);
            return _mapper.Map<{className}Dto>(updatedEntity);
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
