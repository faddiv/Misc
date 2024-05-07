using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Foxy.Params.SourceGenerator.Helpers;
using Foxy.Params.SourceGenerator.Data;
using System.Collections.Immutable;
using System;

namespace Foxy.Params.SourceGenerator
{
    partial class ParamsIncrementalGenerator : IIncrementalGenerator
    {
        private ParamsCandidate? GetSpanParamsMethods(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            SyntaxNode targetNode = context.TargetNode;
            Debug.Assert(targetNode is MethodDeclarationSyntax);
            var decl = Unsafe.As<MethodDeclarationSyntax>(targetNode);

            if (!(context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is IMethodSymbol methodSymbol)
                || !SemanticHelpers.TryGetAttribute(decl, _attributeName, context.SemanticModel, cancellationToken, out var attributeSyntax))
            {
                return null;
            }

            if (HasErrorType(methodSymbol) ||
                HasDuplication(methodSymbol.Parameters))
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

            int maxOverrides = SemanticHelpers.GetValue(context.Attributes.First(), "MaxOverrides", 3);
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

            if (IsOutParameter(spanParam))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.OutModifierNotAllowedDescriptor,
                    attributeSyntax.GetLocation(),
                    methodSymbol.Name, spanParam.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
            }

            if (HasNameCollision(methodSymbol.Parameters, maxOverrides, out string unusableParameters))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.ParameterCollisionDescriptor,
                    attributeSyntax.GetLocation(),
                    methodSymbol.Name, unusableParameters));
            }
            return new ParamsCandidate
            {
                TypeInfo = new TypeCandidate
                {
                    Namespace = nameSpace,
                    TypeName = typeName,
                },
                MethodSymbol = methodSymbol,
                Diagnostics = diagnostics,
                SpanParam = spanParam,
                MaxOverrides = maxOverrides,
                HasParams = SemanticHelpers.GetValue(context.Attributes.First(), "HasParams", true)
            };
        }

        private bool HasNameCollision(ImmutableArray<IParameterSymbol> parameters, int maxOverrides, out string unusableParameters)
        {
            unusableParameters = null;
            if (parameters.Length <= 1)
            {
                return false;
            }
            var spanParameterName = parameters[parameters.Length - 1].Name;
            var collisionParameters = new List<string>
            {
                $"{spanParameterName}Span"
            };
            for (int i = 0; i < maxOverrides; i++)
            {
                collisionParameters.Add($"{spanParameterName}{i}");
            }
            for (int i = 0; i < parameters.Length - 1; i++)
            {
                if (collisionParameters.Contains(parameters[i].Name))
                {
                    unusableParameters = string.Join(", ", collisionParameters);
                    return true;
                }
            }
            return false;
        }

        private bool HasDuplication(ImmutableArray<IParameterSymbol> parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                for (int j = i + 1; j < parameters.Length; j++)
                {
                    if (parameters[i].Name == parameters[j].Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsOutParameter(IParameterSymbol spanParam)
        {
            return spanParam != null
                && spanParam.RefKind == RefKind.Out;
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

