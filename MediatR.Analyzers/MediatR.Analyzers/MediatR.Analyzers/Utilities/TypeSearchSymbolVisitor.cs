using Microsoft.CodeAnalysis;

namespace MediatR.Analyzers.Utilities
{
    public class TypeSearchSymbolVisitor : SymbolVisitor
    {
        public TypeSearchSymbolVisitor()
        {
        }

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
    }
}
