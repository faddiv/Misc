using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Reflection;
using System.Xml.Linq;
using System;

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
            return new ParamsCandidate
            {
                HasErrors = false,
                TypeInfo = new TypeCandidate
                {
                    Namespace = nameSpace,
                    TypeName = typeName,
                },
                AttributeSyntax = attr,
                MethodSymbol = methodSymbol
            };
        }
    }
}

