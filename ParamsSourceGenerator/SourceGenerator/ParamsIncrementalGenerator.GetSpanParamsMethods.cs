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

            string nameSpace = SemanticHelpers.GetNameSpaceNoGlobal(methodSymbol);
            string typeName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var attr = context.Attributes.First();
            var diagnostics = new List<Diagnostic>();
            if(!IsContainingTypePartial(targetNode))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.PartialIsMissingDescriptor,
                    attributeSyntax.GetLocation(),
                    typeName, methodSymbol.Name));
            }
            var spanParam = SemanticHelpers.GetLastParameterOrNull(methodSymbol);
            if(spanParam == null)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.ParameterMissingDescriptor,
                    attributeSyntax.GetLocation(),
                    methodSymbol.Name));
            }
            if (!IsReadOnlySpan(spanParam))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.ParameterMismatchDescriptor,
                    attributeSyntax.GetLocation(),
                    methodSymbol.Name, spanParam.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
            }
            return new ParamsCandidate
            {
                HasErrors = false,
                TypeInfo = new TypeCandidate
                {
                    Namespace = nameSpace,
                    TypeName = typeName,
                },
                AttributeSyntax = attr,
                MethodSymbol = methodSymbol,
                Diagnostics = diagnostics,
                MaxOverrides = SemanticHelpers.GetValue(attr, "MaxOverrides", 3),
                HasParams = SemanticHelpers.GetValue(attr, "HasParams", true)
            };
        }

        private bool IsReadOnlySpan(INamedTypeSymbol spanParam)
        {
            return spanParam == null || spanParam.MetadataName  == "ReadOnlySpan`1";
        }

        private static bool IsContainingTypePartial(SyntaxNode targetNode)
        {
            var containingType = targetNode.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            return containingType?.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword)) ?? false;
        }
    }
}

