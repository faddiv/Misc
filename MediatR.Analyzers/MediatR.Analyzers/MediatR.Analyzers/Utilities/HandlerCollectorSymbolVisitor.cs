using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MediatR.Analyzers.Utilities
{
    public class HandlerCollectorSymbolVisitor : TypeSearchSymbolVisitor
    {
        public readonly List<HandlerInfo> CollectedHandlers = new List<HandlerInfo>();
        private readonly INamedTypeSymbol _requestHandler1;
        private readonly INamedTypeSymbol _requestHandler2;


        public HandlerCollectorSymbolVisitor(INamedTypeSymbol requestHandler1, INamedTypeSymbol requestHandler2)
        {
            _requestHandler1 = requestHandler1;
            _requestHandler2 = requestHandler2;
        }

        public override void VisitNamedType(INamedTypeSymbol namedTypeSymbol)
        {
            if(TypeChecks.TryExtractRequestHandler(
                namedTypeSymbol, _requestHandler1, _requestHandler2, out var handlerInfo))
            {
                CollectedHandlers.Add(handlerInfo);
            }
        }
    }
}
