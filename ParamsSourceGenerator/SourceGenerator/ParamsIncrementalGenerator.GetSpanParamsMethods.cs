using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System;
using Foxy.Params.SourceGenerator.Helpers;
using Foxy.Params.SourceGenerator.Data;

namespace Foxy.Params.SourceGenerator
{
    partial class ParamsIncrementalGenerator : IIncrementalGenerator
    {
        private ParamsCandidate GetSpanParamsMethods(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            SyntaxNode targetNode = context.TargetNode;
            Debug.Assert(targetNode is MethodDeclarationSyntax);
            var decl = Unsafe.As<MethodDeclarationSyntax>(targetNode);

            if (!(context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is IMethodSymbol methodSymbol)
                || !SemanticHelpers.TryGetAttribute(decl, _attributeName, context.SemanticModel, cancellationToken, out var attributeSyntax))
            {
                return null;
            }

            if (HasErrorType(methodSymbol))
            {
                return null;
            }

            string nameSpace = SemanticHelpers.GetNameSpaceNoGlobal(methodSymbol);
            string typeName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var diagnostics = new List<Diagnostic>();
            if (!IsContainingTypePartial(targetNode))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.PartialIsMissingDescriptor,
                    attributeSyntax.GetLocation(),
                    typeName, methodSymbol.Name));
            }
            var spanParam = methodSymbol.Parameters.LastOrDefault();
            var spanType = spanParam?.Type as INamedTypeSymbol;
            if (spanType == null)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.ParameterMissingDescriptor,
                    attributeSyntax.GetLocation(),
                    methodSymbol.Name));
            }
            if (!IsReadOnlySpan(spanType))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.ParameterMismatchDescriptor,
                    attributeSyntax.GetLocation(),
                    methodSymbol.Name, spanParam.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
            }
            return new ParamsCandidate
            {
                TypeInfo = new TypeCandidate
                {
                    Namespace = nameSpace,
                    TypeName = typeName,
                },
                AttributeSyntax = context.Attributes.First(),
                MethodSymbol = methodSymbol,
                Diagnostics = diagnostics,
                SpanParam = spanParam,
                MaxOverrides = SemanticHelpers.GetValue(context.Attributes.First(), "MaxOverrides", 3),
                HasParams = SemanticHelpers.GetValue(context.Attributes.First(), "HasParams", true)
            };
        }

        private bool HasErrorType(IMethodSymbol methodSymbol)
        {
            foreach (var parameter in methodSymbol.Parameters)
            {
                if (parameter.Type.Kind == SymbolKind.ErrorType)
                {
                    return true;
                }
            }

            if (methodSymbol.ReturnType.Kind == SymbolKind.ErrorType)
            {
                return true;

            }

            return false;
        }

        private bool IsReadOnlySpan(INamedTypeSymbol spanParam)
        {
            return spanParam == null || spanParam.MetadataName == "ReadOnlySpan`1";
        }

        private static bool IsContainingTypePartial(SyntaxNode targetNode)
        {
            var containingType = targetNode.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            return containingType?.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword)) ?? false;
        }
    }
}

