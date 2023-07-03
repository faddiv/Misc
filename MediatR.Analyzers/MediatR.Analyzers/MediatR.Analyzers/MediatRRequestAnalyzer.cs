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
        public const string DiagnosticId = "MediatRRequestAnalyzer";

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

                var cache = DiagnosticDataCache.GetInstance(ctx.Compilation);

                ctx.RegisterSymbolAction(ctx2 =>
                {
                    Logger.Log("SymbolAction started {0}", ctx2.Symbol.Name);
                    var local = cache;
                    var compilation = ctx2.Compilation;
                    var symbol = (INamedTypeSymbol)ctx2.Symbol;
                    if (symbol.Name.StartsWith("Message") && !symbol.Name.EndsWith("Handler"))
                    {
                        if (!cache.HasHandler(symbol, compilation))
                        {
                            var diagnostic = Diagnostic.Create(Rule, ctx2.Symbol.Locations[0], ctx2.Symbol.Name);

                            ctx2.ReportDiagnostic(diagnostic);

                        }
                    }
                    // TODO This fires later than the message is checked if this is later in the code.
                    cache.TryAddNewHandler(symbol, compilation);

                    Logger.Log("SymbolAction finished {0}", ctx2.Symbol.Name);
                }, SymbolKind.NamedType);

                Logger.Log("CompilationStartAction finished");
            });
            Logger.Log("Initialize finished");
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext ctx)
        {

            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)ctx.Symbol;
            if (namedTypeSymbol.IsAbstract)
                return;

            if (namedTypeSymbol.IsStatic)
                return;

            var interfaces = namedTypeSymbol.AllInterfaces;

            if (interfaces.Length == 0)
                return;

            var compilation = ctx.Compilation;
            var iRequest1 = compilation.GetTypeByMetadataName("MediatR.IRequest`1");
            if (iRequest1 is null)
                return;
            var correctInterface = interfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(iRequest1, i.OriginalDefinition));
            if (correctInterface is null)
                return;
            var responseTypeSymbol = correctInterface.TypeArguments.FirstOrDefault();
            if (responseTypeSymbol is null)
                return;
            /*var iRequestHandler1 = compilation.GetTypeByMetadataName("MediatR.IRequestHandler`1");
            if (iRequestHandler1 is null)
                return;*/

            var iRequestHandler2 = compilation.GetTypeByMetadataName("MediatR.IRequestHandler`2");
            if (iRequestHandler2 is null)
                return;
            var concretehandler = iRequestHandler2.Construct(namedTypeSymbol, responseTypeSymbol);
            var handleMethod = concretehandler.GetMembers("Handle").FirstOrDefault();
            if (handleMethod is null)
                return;

            var implementation = concretehandler.FindImplementationForInterfaceMember(handleMethod);

            if (implementation is null)
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                ctx.ReportDiagnostic(diagnostic);

            }
        }
    }
}
