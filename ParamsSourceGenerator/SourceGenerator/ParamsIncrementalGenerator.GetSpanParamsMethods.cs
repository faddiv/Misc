using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Foxy.Params.SourceGenerator
{
    partial class ParamsIncrementalGenerator : IIncrementalGenerator
    {
        private ParamsCandidate GetSpanParamsMethods(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            Debug.Assert(context.TargetNode is MethodDeclarationSyntax);
            var decl = Unsafe.As<MethodDeclarationSyntax>(context.TargetNode);

            if (!(context.SemanticModel.GetDeclaredSymbol(decl, cancellationToken) is IMethodSymbol methodSymbol)
                || !SemanticHelpers.TryGetAttribute(decl, _attributeName, context.SemanticModel, cancellationToken, out var attributeSyntax))
            {
                return null;
            }

            string nameSpace = SemanticHelpers.GetNameSpaceNoGlobal(methodSymbol);
            string typeName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var attr = context.Attributes.First();
            var diagnostics = new List<Diagnostic>();
            var containingType = context.TargetNode.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if(!containingType?.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword)) ?? false)
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticReports.PartialIsMissingDescriptor,
                    attributeSyntax.GetLocation(),
                    typeName, methodSymbol.Name));
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
                Diagnostics = diagnostics 
            };
        }
    }
}

