using Microsoft.CodeAnalysis;
using System;

namespace Rdd.Analyzer.Helpers
{
    public class RddDomainContext
    {
        readonly Lazy<IPropertySymbol> lazyIEntityBaseUrlMember;

        public RddDomainContext(Compilation compilation)
        {
            lazyIEntityBaseUrlMember = new Lazy<IPropertySymbol>(() => compilation.GetTypeByMetadataName("Rdd.Domain.IEntityBase").GetMembers("Url")[0] as IPropertySymbol);
        }

        public IPropertySymbol IEntityBaseUrlMember => lazyIEntityBaseUrlMember?.Value;
    }
}