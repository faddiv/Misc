using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Foxy.Params.SourceGenerator.Generators
{
    using static SyntaxFactory;

    internal static class GeneratorHelpers
    {
        public static StructDeclarationSyntax ExtendWithMembers(
            this StructDeclarationSyntax syntax, params MemberDeclarationSyntax[] members)
        {
            return syntax.WithMembers(List(members));
        }

        public static StructDeclarationSyntax WithTypeParameters(this StructDeclarationSyntax syntax, string typedParameterName)
        {
            return syntax
                .WithTypeParameterList(
                    TypeParameterList(
                        SingletonSeparatedList(
                            TypeParameter(
                                Identifier(typedParameterName)
                            )
                        )
                    )
                );
        }


        public static ConstructorDeclarationSyntax ExtendWithParameters(
            this ConstructorDeclarationSyntax syntax,
            IEnumerable<ParameterSyntax> parameters)
        {
            return syntax.WithParameterList(ParameterList(SeparatedList(parameters)));
        }
    }
}
