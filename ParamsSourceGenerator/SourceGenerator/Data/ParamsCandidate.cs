using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator.Data
{
    internal class ParamsCandidate
    {
        public bool HasErrors => Diagnostics.Count > 0;

        public IMethodSymbol MethodSymbol { get; set; }

        public AttributeData AttributeSyntax { get; set; }

        public TypeCandidate TypeInfo { get; set; }

        public INamedTypeSymbol LastParameter { get; set; }

        public List<Diagnostic> Diagnostics { get; internal set; }
        public int MaxOverrides { get; internal set; }
        public bool HasParams { get; internal set; }
    }
}

