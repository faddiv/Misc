using Foxy.Params.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator.Data
{
    internal class TypeCandidate(INamedTypeSymbol containingType) : IEquatable<TypeCandidate>
    {
        public INamedTypeSymbol ContainingType { get; } = containingType;

        public static string CreateFileName(INamedTypeSymbol containingType)
        {
                return $"{containingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}.g.cs";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeCandidate);
        }

        public bool Equals(TypeCandidate? other)
        {
            return !(other is null) &&
                    SymbolEqualityComparer.Default.Equals(ContainingType, other.ContainingType);
        }

        public override int GetHashCode()
        {
            return SymbolEqualityComparer.Default.GetHashCode(ContainingType);
        }

        public static bool operator ==(TypeCandidate left, TypeCandidate right)
        {
            return EqualityComparer<TypeCandidate>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeCandidate left, TypeCandidate right)
        {
            return !(left == right);
        }
    }
}

