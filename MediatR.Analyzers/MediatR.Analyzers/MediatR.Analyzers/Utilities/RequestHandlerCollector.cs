using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Threading;

namespace MediatR.Analyzers.Utilities
{
    public class RequestHandlerCollector : SymbolVisitor
    {
        private static volatile int _idGen = 0;
        public int id = Interlocked.Increment(ref _idGen);

        public readonly ConcurrentBag<INamedTypeSymbol> Handlers = new ConcurrentBag<INamedTypeSymbol>();
        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            foreach (var item in symbol.Modules)
            {
                VisitModule(item);
            }
        }
        public override void VisitModule(IModuleSymbol symbol)
        {
            VisitNamespace(symbol.GlobalNamespace);
        }
        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var item in symbol.GetMembers())
            {
                if (item is INamespaceSymbol subns)
                {
                    VisitNamespace(subns);
                }
                else if (item is INamedTypeSymbol type)
                {
                    VisitNamedType(type);
                }
            }
        }
        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.Name.EndsWith("Handler"))
            {
                Handlers.Add(symbol);
            }
            base.VisitNamedType(symbol);
        }
    }
}
