using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foxy.Params.SourceGenerator.Helpers
{
    internal static class SemanticHelpers
    {
        public static bool TryGetAttribute(
            MemberDeclarationSyntax candidate,
            string attributeName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out AttributeSyntax value)
        {
            foreach (AttributeListSyntax attributeList in candidate.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    SymbolInfo info = semanticModel.GetSymbolInfo(attribute, cancellationToken);
                    ISymbol symbol = info.Symbol;

                    if (symbol is IMethodSymbol method
                        && method.ContainingType.ToDisplayString().Equals(attributeName, StringComparison.Ordinal))
                    {
                        value = attribute;
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        public static string GetNameSpaceNoGlobal(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.ContainingNamespace.IsGlobalNamespace)
                return "";
            var nameSpacesParts = methodSymbol.ContainingNamespace.ToDisplayParts(SymbolDisplayFormat.FullyQualifiedFormat);
            var nameSpace = string.Join("", nameSpacesParts.Skip(2));
            return nameSpace;
        }

        public static string GetNameSpaceGlobal(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.ContainingNamespace.IsGlobalNamespace)
                return "";
            return methodSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public static T GetValue<T>(AttributeData attributeSyntax, string argumentName, T defaultValue)
        {
            foreach (var item in attributeSyntax.NamedArguments)
            {
                if (item.Key != argumentName)
                    continue;

                return (T)item.Value.Value;
            }

            return defaultValue;
        }

        public static INamedTypeSymbol GetLastParameterOrNull(IMethodSymbol methodSymbol)
        {
            var lastParam = methodSymbol.Parameters.LastOrDefault();
            return lastParam?.Type as INamedTypeSymbol;
        }
    }
}

