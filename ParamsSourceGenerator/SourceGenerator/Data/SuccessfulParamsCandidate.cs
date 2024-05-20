using Microsoft.CodeAnalysis;

namespace Foxy.Params.SourceGenerator.Data
{
    internal class SuccessfulParamsCandidate : ParamsCandidate
    {
        public override bool HasErrors => false;

        public required IMethodSymbol MethodSymbol { get; init; }

        public required INamedTypeSymbol ContainingType { get; init; }

        public required IParameterSymbol SpanParam { get; init; }

        public required int MaxOverrides { get; init; }

        public required bool HasParams { get; init; }
    }
}

