using Microsoft.CodeAnalysis;

namespace Foxy.Params.SourceGenerator.Data
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

        public static DiagnosticDescriptor ParameterMissingDescriptor { get; } = new DiagnosticDescriptor(
            "PRM1001",
            "Parameter missing",
            "The method '{0}' must have ReadOnlySpan<> as last parameter but no parameter found.",
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

        public static DiagnosticDescriptor OutModifierNotAllowedDescriptor { get; } = new DiagnosticDescriptor(
            "PRM1003",
            "Out modifier not allowed",
            "The last parameter on method '{0}' can't have out modifier. Found: '{1}'",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor ParameterCollisionDescriptor { get; } = new DiagnosticDescriptor(
            "PRM1004",
            "Parameter collision",
            "The following parameter names can't be used on the method '{0}': '{1}'",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
