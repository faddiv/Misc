using System.Threading;
using Foxy.Params.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                .NotNull()
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

