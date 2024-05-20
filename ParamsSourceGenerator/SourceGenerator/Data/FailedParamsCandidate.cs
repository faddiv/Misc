using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator.Data
{
    internal class FailedParamsCandidate : ParamsCandidate
    {
        public override bool HasErrors => true;

        public required List<Diagnostic> Diagnostics { get; init; }
    }
}

