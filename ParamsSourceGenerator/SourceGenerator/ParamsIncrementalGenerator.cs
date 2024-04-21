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
    [Generator]
    public partial class ParamsIncrementalGenerator : IIncrementalGenerator
    {
        private const string _attributeName = "Foxy.Params.ParamsAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(AddParamsAttribute);
            var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                _attributeName,
                predicate: Filter,
                transform: GetSpanParamsMethods)
                .Collect();

            context.RegisterSourceOutput(declarations, GenerateSource);
        }

        private void AddParamsAttribute(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("ParamsAttribute.g.cs", _paramsAttribute);
        }

        private static bool Filter(SyntaxNode s, CancellationToken token)
        {
            return s is MethodDeclarationSyntax methodDeclarationSyntax;
        }
    }
}

