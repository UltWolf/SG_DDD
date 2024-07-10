//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//namespace SourceGeneratorLib.Generators
//{
//    public class AutoGenerator
//    {


//#pragma warning disable RS1035

//        [Generator]
//        public class AutoGeneratora : ISourceGenerator
//        {
//            public void Initialize(GeneratorInitializationContext context)
//            {
//                // No initialization required for this example
//            }

//            public void Execute(GeneratorExecutionContext context)
//            {
//                // Get the syntax trees in the compilation
//                var syntaxTrees = context.Compilation.SyntaxTrees;

//                // Define the base output directory
//                var baseOutputDir = "GeneratedOutput";

//                // Create the base output directory if it doesn't exist
//                if (!Directory.Exists(baseOutputDir))
//                {
//                    Directory.CreateDirectory(baseOutputDir);
//                }

//                // Loop through all syntax trees
//                foreach (var syntaxTree in syntaxTrees)
//                {
//                    // Get the root syntax node
//                    var root = syntaxTree.GetRoot();

//                    // Find all class declarations with the GenerateCode attribute
//                    var classDeclarations = root.DescendantNodes()
//                        .OfType<ClassDeclarationSyntax>()
//                        .Where(classDecl => classDecl.AttributeLists
//                            .SelectMany(attrList => attrList.Attributes)
//                            .Any(attr => attr.Name.ToString() == "GenerateCode"));

//                    foreach (var classDeclaration in classDeclarations)
//                    {
//                        // Get the namespace
//                        var @namespace = GetNamespace(classDeclaration);

//                        // Get the class name
//                        var className = classDeclaration.Identifier.Text;

//                        // Generate the repository code
//                        var repositorySource = GenerateRepository(@namespace, className);
//                        WriteToFile(baseOutputDir, $"{className}_Repository.cs", repositorySource);

//                        // Generate the DTO code
//                        var dtoSource = GenerateDTO(@namespace, className);
//                        WriteToFile(baseOutputDir, $"{className}_DTO.cs", dtoSource);

//                        // Generate the AutoMapper configuration code
//                        var autoMapperConfigSource = GenerateAutoMapperConfig(@namespace, className);
//                        WriteToFile(baseOutputDir, $"{className}_AutoMapperConfig.cs", autoMapperConfigSource);

//                        // Generate the entity code
//                        var entitySource = GenerateEntity(@namespace, className);
//                        WriteToFile(baseOutputDir, $"{className}_Entity.cs", entitySource);

//                        // Generate the EF configuration code
//                        var efConfigSource = GenerateEFConfig(@namespace, className);
//                        WriteToFile(baseOutputDir, $"{className}_EFConfig.cs", efConfigSource);
//                    }
//                }
//            }

//            private string GetNamespace(SyntaxNode classDeclaration)
//            {
//                // Find the namespace declaration for the class
//                var namespaceDeclaration = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
//                return namespaceDeclaration?.Name.ToString() ?? "GlobalNamespace";
//            }

//            private string GenerateRepository(string @namespace, string className)
//            {
//                return $@"
//namespace {@namespace}.Repositories
//{{
//    public interface I{className}Repository
//    {{
//        // Interface methods for {className} repository
//    }}

//    public partial class {className}Repository : I{className}Repository
//    {{
//        // Partial class implementation for {className} repository
//    }}
//}}";
//            }

//            private string GenerateDTO(string @namespace, string className)
//            {
//                return $@"
//namespace {@namespace}.DTOs
//{{
//    public partial class {className}DTO
//    {{
//        // Partial class implementation for {className} DTO
//    }}
//}}";
//            }

//            private string GenerateAutoMapperConfig(string @namespace, string className)
//            {
//                return $@"
//namespace {@namespace}.AutoMapperConfigs
//{{
//    public partial class {className}AutoMapperConfig : Profile
//    {{
//        public {className}AutoMapperConfig()
//        {{
//            CreateMap<{className}, {className}DTO>();
//            CreateMap<{className}DTO, {className}>();
//        }}
//    }}
//}}";
//            }

//            private string GenerateEntity(string @namespace, string className)
//            {
//                return $@"
//namespace {@namespace}.Entities
//{{
//    public partial class {className}Entity
//    {{
//        // Partial class implementation for {className} entity
//    }}
//}}";
//            }

//            private string GenerateEFConfig(string @namespace, string className)
//            {
//                return $@"
//namespace {@namespace}.EFConfigurations
//{{
//    public partial class {className}EFConfig : IEntityTypeConfiguration<{className}Entity>
//    {{
//        public void Configure(EntityTypeBuilder<{className}Entity> builder)
//        {{
//            // EF configuration code for {className} entity
//        }}
//    }}
//}}";
//            }

//            private void WriteToFile(string directory, string fileName, string content)
//            {
//                var filePath = Path.Combine(directory, fileName);

//                // Check if the file already exists
//                if (File.Exists(filePath))
//                {
//                    return;
//                }

//                File.WriteAllText(filePath, content);
//            }
//        }

//#pragma warning restore RS1035

//    }
//}
