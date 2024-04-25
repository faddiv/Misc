using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator
{
    partial class ParamsIncrementalGenerator : IIncrementalGenerator
    {
        private void GenerateSource(SourceProductionContext context, ImmutableArray<ParamsCandidate> typeSymbols)
        {
            foreach (var uniqueClass in typeSymbols
                .Where(e => !e.HasErrors)
                .GroupBy(e => e.TypeInfo))
            {
                var typeInfo = uniqueClass.Key;
                var sb = new SourceBuilder();
                sb.AutoGenerated();
                sb.AppendLine();
                sb.NullableEnable();
                sb.AppendLine();
                sb.Namespace(typeInfo.Namespace);
                sb.Class(typeInfo.TypeName);
                var maxOverridesMax = 0;
                foreach (var item in uniqueClass)
                {
                    var maxOverrides = SemanticHelpers.GetValue(item.AttributeSyntax, "MaxOverrides", 3);
                    string name = item.MethodSymbol.Name;
                    var spanArgumentType = GetSpanArgumentType(item.MethodSymbol);
                    var argumentInfos = GetNonParamsArguments(item.MethodSymbol);
                    var fixArguments = argumentInfos.Select(e => e.ToParameter()).ToList();
                    var returnsVoid = item.MethodSymbol.ReturnsVoid;
                    var isGenericMethod = item.MethodSymbol.IsGenericMethod;
                    var typeArguments = item.MethodSymbol.TypeArguments.Select(e => e.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToList();
                    string returnType = item.MethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var argName = "args";
                    var isStatic = item.MethodSymbol.IsStatic;

                    for (int i = 1; i <= maxOverrides; i++)
                    {
                        if (i > 1)
                        {
                            sb.AppendLine();
                        }

                        var variableArguments = Enumerable.Range(0, i).Select(j => $"{spanArgumentType} {argName}{j}");
                        sb.Method(
                            name,
                            fixArguments.Concat(variableArguments),
                            isStatic,
                            returnType,
                            typeArguments);
                        sb.AppendLine($"var foxyParamsArray = new Arguments{i}<{spanArgumentType}>({string.Join(", ", Enumerable.Range(0, i).Select(j => $"{argName}{j}"))});");
                        CreateCallLine(
                            sb,
                            name,
                            argumentInfos,
                            returnsVoid,
                            typeArguments,
                            $"global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref foxyParamsArray.arg0, {i})");
                        sb.CloseBlock();
                    }
                    var hasParams = SemanticHelpers.GetValue(item.AttributeSyntax, "HasParams", true);
                    if (hasParams)
                    {
                        sb.AppendLine();
                        sb.Method(
                            name,
                            fixArguments.Append($"params {spanArgumentType}[] {argName}"),
                            isStatic,
                            returnType,
                            typeArguments);
                        CreateCallLine(
                            sb,
                            name,
                            argumentInfos,
                            returnsVoid,
                            typeArguments,
                            $"new global::System.ReadOnlySpan<{spanArgumentType}>(args)");
                        sb.CloseBlock();
                    }
                    maxOverridesMax = Math.Max(maxOverridesMax, maxOverrides);
                }
                sb.CloseBlock();
                for (int i = 1; i <= maxOverridesMax; i++)
                {
                    sb.AppendLine();
                    CreateArguments(sb, i);
                }
                sb.CloseBlock();
                context.AddSource(uniqueClass.Key.CreateFileName(), SourceText.From(sb.ToString(), Encoding.UTF8));
            }

        }

        private static void CreateCallLine(
            SourceBuilder sb,
            string name,
            List<ArgumentInfo> argumentInfos,
            bool returnsVoid,
            List<string> typeArguments,
            string paramsArgument)
        {
            var codeLine = sb.StartLine();
            if (!returnsVoid)
            {
                codeLine.Returns();
            }
            codeLine.AddSegment(name);
            if(typeArguments.Count > 0)
            {
                codeLine.AddSegment("<");
                codeLine.AddCommaSeparatedList(typeArguments);
                codeLine.AddSegment(">");
            }
            codeLine.AddSegment("(");
            codeLine.AddCommaSeparatedList(argumentInfos.Select(e => e.Name));
            codeLine.AddSegment($", {paramsArgument})");
            codeLine.EndLine();
        }

        private string GetSpanArgumentType(IMethodSymbol methodSymbol)
        {
             var spanParam = methodSymbol.Parameters.Last();
            var spanType = spanParam.Type as INamedTypeSymbol;
            var spanTypeArgument = spanType.TypeArguments.First();
            return spanTypeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static List<ArgumentInfo> GetNonParamsArguments(IMethodSymbol methodSymbol)
        {
            var parameters = new List<ArgumentInfo>();
            foreach (var arg in methodSymbol.Parameters.Take(methodSymbol.Parameters.Length - 1))
            {
                var type = arg.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var name = arg.Name;
                parameters.Add(new ArgumentInfo
                {
                    Name = name,
                    Type = type,
                });
            }
            return parameters;
        }

        private void CreateArguments(SourceBuilder sb, int length)
        {
            sb.Attribute($"System.Runtime.CompilerServices.InlineArray({length})");
            sb.GenericStruct($"Arguments{length}", "T");
            sb.Field("T", "arg0");
            sb.AppendLine();
            sb.Constructor(Enumerable.Range(0, length).Select(e => $"T value{e}"));
            sb.AppendLine($"arg0 = value0;");
            for (int i = 1; i < length; i++)
            {
                sb.AppendLine($"this[{i}] = value{i};");
            }
            sb.CloseBlock();
            sb.CloseBlock();
        }
    }

    public class ArgumentInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }

        public string ToParameter()
        {
            return $"{Type} {Name}";
        }
    }
}

