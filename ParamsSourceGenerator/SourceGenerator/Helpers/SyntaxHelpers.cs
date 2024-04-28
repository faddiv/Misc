using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foxy.Params.SourceGenerator.Helpers
{
    internal static class SyntaxHelpers
    {
        public static bool HasAttribute(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return methodDeclarationSyntax.AttributeLists.Count > 0;
        }
    }
}

