namespace SourceGenerator.ApplicationLevel.Generators
{
    using Microsoft.CodeAnalysis;
    using SourceGenerator.Common.Data;
    using SourceGenerator.Common.Helper;
#pragma warning disable RS1035
    namespace SourceGeneratorLib.Generators
    {
        [Generator]
        public class MediatRCommandGenerator : ISourceGenerator
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
                            var classDeclarations = ClassDeclarationHelper.GetClassDeclarations(domainAssembly.GlobalNamespace, "MediatRCommand").ToList();

                            foreach (var classDeclaration in classDeclarations)
                            {
                                var @namespace = classDeclaration.ContainingNamespace.ToDisplayString();
                                var className = classDeclaration.Name;

                                var baseOutputDir = Path.Combine(className.Trim(), "Commands");

                                var classToInsert = new ClassesToInsert
                                {
                                    ClassName = className,
                                    GeneratedClasses = new List<GeneratedClass>
                                {
                                    new GeneratedClass
                                    {
                                        ClassName = $"Create{className}Command.cs",
                                        Generated = GenerateCreateCommand(@namespace, className),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Delete{className}Command.cs",
                                        Generated = GenerateDeleteCommand(@namespace, className),
                                        PathToOutput = baseOutputDir
                                    },
                                    new GeneratedClass
                                    {
                                        ClassName = $"Update{className}Command.cs",
                                        Generated = GenerateUpdateCommand(@namespace, className),
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



            private bool HasGenerateCodeAttribute(INamedTypeSymbol classDeclaration, string attributeValue)
            {
                var generateCodeAttribute = classDeclaration.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass.ToDisplayString().Contains("Data.Attributes.GenerateCode") &&
                    attr.ConstructorArguments.Length == 1 &&
                    attr.ConstructorArguments[0].Value is string value &&
                    value == attributeValue);

                return generateCodeAttribute != null;
            }

            private string GenerateCreateCommand(string namespaceName, string className)
            {
                return $@"
using MediatR;
using OneOf;
using {namespaceName}.DTO;
using {namespaceName}.Service;
using {namespaceName}.Entities;
using {namespaceName}.Repositories;
using {namespaceName}.Errors;

namespace {namespaceName}.Commands
{{
    public class Create{className}Command : IRequest<OneOf<{className}Dto, TrainingError>>
    {{
        public Create{className}Dto New{className} {{ get; init; }} = null!;
    }}

    public class Create{className}Handler : IRequestHandler<Create{className}Command, OneOf<{className}Dto, TrainingError>>
    {{
        private readonly IMapper _mapper;
        private readonly I{className}Repository _repository; 

        public Create{className}Handler(I{className}Repository repository, IMapper mapper)
        {{
            _mapper = mapper;
            _repository = repository;
            _service = service;
        }}

        public async Task<OneOf<{className}Dto, TrainingError>> Handle(Create{className}Command request, CancellationToken cancellationToken)
        {{
            var entity = _mapper.Map<{className}>(request.New{className});
            var newEntity = await _repository.AddAsync(entity, cancellationToken);
            return _mapper.Map<{className}Dto>(newEntity);
        }}
    }}
}}
";
            }

            private string GenerateDeleteCommand(string namespaceName, string className)
            {
                return $@"
using MediatR;
using {namespaceName}.Entities;
using {namespaceName}.Repositories;

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
            return _repository.DeleteAsync(new {className}() {{ Id = new {className}Id(request.{className}Id) }}, cancellationToken);
        }}
    }}
}}
";
            }

            private string GenerateUpdateCommand(string namespaceName, string className)
            {
                return $@"
using MediatR;
using OneOf;
using {namespaceName}.DTO;
using {namespaceName}.Service;
using {namespaceName}.Entities;
using {namespaceName}.Repositories;
using {namespaceName}.Errors;

namespace {namespaceName}.Commands
{{
    public class Update{className}Command : IRequest<OneOf<{className}Dto, TrainingError>>
    {{
        public {className}Dto Update{className} {{ get; init; }} = null!;
    }}

    public class Update{className}Handler : IRequestHandler<Update{className}Command, OneOf<{className}Dto, TrainingError>>
    {{
        private readonly IMapper _mapper;
        private readonly I{className}Repository _repository;
        private readonly I{className}Service _service;

        public Update{className}Handler(I{className}Repository repository, IMapper mapper, I{className}Service service)
        {{
            _mapper = mapper;
            _repository = repository;
            _service = service;
        }}

        public async Task<OneOf<{className}Dto, TrainingError>> Handle(Update{className}Command request, CancellationToken cancellationToken)
        {{
            var entity = _mapper.Map<{className}>(request.Update{className});
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

}
