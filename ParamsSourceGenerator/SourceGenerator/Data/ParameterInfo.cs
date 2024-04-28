using Foxy.Params.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System;

namespace Foxy.Params.SourceGenerator.Data
{
    public class ParameterInfo
    {
        public ParameterInfo(IParameterSymbol arg)
        {
            Type = arg.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            Name = arg.Name;
            RefKind = arg.RefKind;
            IsNullable = arg.NullableAnnotation == NullableAnnotation.Annotated;
        }

        public string Type { get; }
        public string Name { get; }
        public RefKind RefKind { get; }
        public bool IsNullable { get; }

        public string ToParameter()
        {
            return $"{SemanticHelpers.WithModifiers(Type, RefKind, IsNullable)} {Name}";
        }

        public string ToPassParameter()
        {
            return SemanticHelpers.WithModifiers(Name, GetPassParameterModifier(RefKind), false);
        }

        private static RefKind GetPassParameterModifier(RefKind refKind)
        {
            switch (refKind)
            {
                case RefKind.Ref:
                    return RefKind.Ref;
                case RefKind.Out:
                    return RefKind.Out;
                case RefKind.In:
                case RefKind.RefReadOnlyParameter:
                    return RefKind.In;
                default:
                    return RefKind.None;
            }
        }
    }
}

