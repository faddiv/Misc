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
        }

        public string Type { get; }
        public string Name { get; }
        public RefKind RefKind { get; }

        public string ToParameter()
        {
            return $"{GetParameterModifier(RefKind)}{Type} {Name}";
        }

        public string ToPassParameter()
        {
            return $"{GetPassParameterModifier(RefKind)}{Name}";
        }

        private static string GetParameterModifier(RefKind refKind)
        {
            switch (refKind)
            {
                case RefKind.Ref:
                    return "ref ";
                case RefKind.Out:
                    return "out ";
                case RefKind.In:
                    return "in ";
                case RefKind.RefReadOnlyParameter:
                    return "ref readonly ";
                default:
                    return "";
            }
        }

        private static string GetPassParameterModifier(RefKind refKind)
        {
            switch (refKind)
            {
                case RefKind.Ref:
                    return "ref ";
                case RefKind.Out:
                    return "out ";
                case RefKind.In:
                case RefKind.RefReadOnlyParameter:
                    return "in ";
                default:
                    return "";
            }
        }
    }
}

