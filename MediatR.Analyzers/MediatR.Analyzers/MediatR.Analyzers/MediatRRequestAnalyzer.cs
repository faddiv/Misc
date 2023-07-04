using MediatR.Analyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static MediatR.Analyzers.Utilities.Constants;

namespace MediatR.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MediatRRequestAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MR1000";

        public const string Category = "MediatR";

        public static DiagnosticDescriptor Rule { get; } = new DiagnosticDescriptor(
            DiagnosticId,
            "No handler found",
            "The mediatr request {0} doesn't have any handler",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "All mediatr request should have handler.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            Debugger.Launch();
            Logger.Log("Initialize started");
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(ctx =>
            {
                Logger.Log("CompilationStartAction started");
                DiagnosticDataCache.GetInstance(ctx.Compilation);

                ctx.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);

                Logger.Log("CompilationStartAction finished");
            });
            Logger.Log("Initialize finished");
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext ctx2)
        {
            Logger.Log("SymbolAction started {0}", ctx2.Symbol.Name);
            var compilation = ctx2.Compilation;
            var cache = DiagnosticDataCache.GetInstance(compilation);
            var symbol = (INamedTypeSymbol)ctx2.Symbol;
            if (TypeChecks.IsRequest(symbol, compilation))
            {
                if (!cache.HasHandler(symbol, compilation))
                {
                    var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);

                    ctx2.ReportDiagnostic(diagnostic);

                }
            }

            cache.TryAddNewHandler(symbol, compilation);

            Logger.Log("SymbolAction finished {0}", ctx2.Symbol.Name);
        }
    }
}
