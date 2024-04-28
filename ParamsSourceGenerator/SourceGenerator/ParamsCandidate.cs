using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator
{
    internal class ParamsCandidate
    {
        public IMethodSymbol MethodSymbol { get; set; }
        public AttributeData AttributeSyntax { get; set; }
        public TypeCandidate TypeInfo { get; set; }
        public bool HasErrors { get; internal set; }
        public List<Diagnostic> Diagnostics { get; internal set; }
    }
}

