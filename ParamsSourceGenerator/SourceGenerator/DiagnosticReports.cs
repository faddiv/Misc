using Microsoft.CodeAnalysis;

namespace Foxy.Params.SourceGenerator
{
    public class DiagnosticReports
    {
        public const string Category = "Foxy.ParamsGenerator";

        public static DiagnosticDescriptor PartialIsMissingDescriptor { get; } = new DiagnosticDescriptor(
            "PRM1000",
            "Class is not partial",
            "The class '{0}' must have partial keyword to create the params overrides for {1}.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor ParameterMismatchDescriptor { get; } = new DiagnosticDescriptor(
            "PRM1002",
            "Parameter mismatch",
            "The method '{0}' must have ReadOnlySpan<> as last parameter. Found: '{1}'",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
