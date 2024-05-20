using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Foxy.Params.SourceGenerator.Data;
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

        public static string GetNameSpaceNoGlobal(ISymbol? symbol)
        {
            if (symbol is null ||
                symbol.ContainingNamespace.IsGlobalNamespace)
                return "";
            var nameSpacesParts = symbol.ContainingNamespace.ToDisplayParts(SymbolDisplayFormat.FullyQualifiedFormat);
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

        public static ReturnKind GetReturnsKind(IMethodSymbol methodSymbol)
        {
            if(methodSymbol.ReturnsVoid)
            {
                return ReturnKind.ReturnsVoid;
            }
            if(methodSymbol.ReturnsByRef || methodSymbol.ReturnsByRefReadonly)
            {
                return ReturnKind.ReturnsRef;
            }
            return ReturnKind.ReturnsType;
        }

        public static List<INamedTypeSymbol> GetTypeHierarchy(INamedTypeSymbol? containingType)
        {
            var list = new List<INamedTypeSymbol>();
            while (containingType is not null)
            {
                list.Add(containingType);
                containingType = containingType.ContainingType;
            }
            list.Reverse();
            return list;
        }
        public static string CreateFileName(INamedTypeSymbol containingType)
        {
            return $"{containingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}.g.cs";
        }

        public static string WithModifiers(string typeName, RefKind refKind, bool isNullable)
        {
            switch (refKind)
            {
                case RefKind.Ref:
                    return $"ref {typeName}{GetNullableModifier(isNullable)}";
                case RefKind.Out:
                    return $"out {typeName}{GetNullableModifier(isNullable)}";
                case RefKind.In:
                    return $"in {typeName}{GetNullableModifier(isNullable)}";
                case RefKind.RefReadOnlyParameter:
                    return $"ref readonly {typeName}{GetNullableModifier(isNullable)}";
                default:
                    if (!isNullable)
                    {
                        return typeName;
                    }
                    return $"{typeName}?";
            }
        }

        private static string GetNullableModifier(bool isNullable)
        {
            return isNullable ? "?" : "";
        }
    }
}

