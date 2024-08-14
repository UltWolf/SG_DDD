using Microsoft.CodeAnalysis;
using SourceGenerator.Common.Data.Constants;

namespace SourceGenerator.APILevel.Helpers
{
    public class ReferenceHelper
    {
        public static AssemblyRefs GetAdditionalNamespaces(Compilation compilation)
        {
            var assemblyRef = new AssemblyRefs();

            var applicationReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Application"));
            if (applicationReference != null)
            {
                var applicationRef = compilation.GetAssemblyOrModuleSymbol(applicationReference) as IAssemblySymbol;
                assemblyRef.ApplicationName = applicationRef.Name.ToString();
            }

            var domainsReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Domain"));
            if (domainsReference != null)
            {
                assemblyRef.DomainName = (compilation.GetAssemblyOrModuleSymbol(domainsReference) as IAssemblySymbol).Name.ToString();
            }

            var infrastructureReference = compilation.References.FirstOrDefault(r => r.Display.Contains(".Infrastructure"));
            if (infrastructureReference != null)
            {
                assemblyRef.InfrastructureName = (compilation.GetAssemblyOrModuleSymbol(infrastructureReference) as IAssemblySymbol).Name.ToString();

            }

            return assemblyRef;
        }
    }
}
