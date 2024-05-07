using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator.Data
{
    internal class ParamsCandidate
    {
        public bool HasErrors => Diagnostics.Count > 0;

        public IMethodSymbol MethodSymbol { get; set; } = default!;

        public TypeCandidate TypeInfo { get; set; } = default!;

        public IParameterSymbol SpanParam { get; internal set; } = default!;

        public List<Diagnostic> Diagnostics { get; internal set; } = default!;

        public int MaxOverrides { get; internal set; }

        public bool HasParams { get; internal set; }
    }
}

