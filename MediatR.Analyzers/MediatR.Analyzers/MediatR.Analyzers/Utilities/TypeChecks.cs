using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MediatR.Analyzers.Utilities.Constants;

namespace MediatR.Analyzers.Utilities
{
    internal class TypeChecks
    {
        public static bool TryExtractRequestHandler(
            INamedTypeSymbol namedTypeSymbol,
            INamedTypeSymbol _requestHandler1,
            INamedTypeSymbol _requestHandler2,
            out HandlerInfo handlerInfo)
        {
            handlerInfo = null;

            if (namedTypeSymbol.IsAbstract ||
                namedTypeSymbol.IsStatic)
                return false;

            var interfaces = namedTypeSymbol.AllInterfaces;

            if (interfaces.Length == 0)
                return false;

            var correctInterface = interfaces.FirstOrDefault(i =>
            SymbolEqualityComparer.Default.Equals(_requestHandler1, i.OriginalDefinition) ||
            SymbolEqualityComparer.Default.Equals(_requestHandler2, i.OriginalDefinition));
            if (correctInterface is null)
                return false;

            if (correctInterface.TypeArguments.Length == 0)
                return false;

            if (correctInterface.TypeArguments[0] is INamedTypeSymbol request)
            {
                handlerInfo = new HandlerInfo(
                    namedTypeSymbol.ToDisplayString(),
                    request.ToDisplayString());
                return true;
            }
            return false;
        }

        public static INamedTypeSymbol GetRequestHandler1(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(IRequestHandler1);
        }

        public static INamedTypeSymbol GetRequestHandler2(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(IRequestHandler2);
        }

        internal static bool IsRequest(INamedTypeSymbol namedTypeSymbol, Compilation compilation)
        {
            var request = compilation.GetTypeByMetadataName(IRequest);
            if (request is null)
                return false;
            var request1 = compilation.GetTypeByMetadataName(IRequest1);
            if (request1 is null)
                return false;

            if (namedTypeSymbol.IsAbstract ||
                namedTypeSymbol.IsStatic)
                return false;

            var interfaces = namedTypeSymbol.AllInterfaces;

            if (interfaces.Length == 0)
                return false;

            var correctInterface = interfaces.FirstOrDefault(i =>
            SymbolEqualityComparer.Default.Equals(request, i.OriginalDefinition) ||
            SymbolEqualityComparer.Default.Equals(request1, i.OriginalDefinition));
            if (correctInterface is null)
                return false;

            return true;
        }
    }
}
