using Microsoft.CodeAnalysis;

namespace Foxy.Params.SourceGenerator
{
    public class DiagnosticReports
    {
        public const string PartialIsMissingId = "PRM1000";
        public const string Category = "Foxy.ParamsGeneration";

        public static DiagnosticDescriptor PartialIsMissingDescriptor { get; } = new DiagnosticDescriptor(
            PartialIsMissingId,
            "Class is not partial",
            "The class '{0}' must have partial keyword to create the params overrides for {1}.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
