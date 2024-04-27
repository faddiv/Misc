using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Drawing;
using System.Linq;

namespace Foxy.Params.SourceGenerator.Generators
{
    using static SyntaxFactory;

    internal static class ArgumentClassGenerator
    {
        public static SyntaxNode Generate(int size)
        {
            var result = CreateStructArgumensOf(size);
            return result;
        }

        public static StructDeclarationSyntax CreateStructArgumensOf(int size)
        {
            var className = $"Arguments{size}";
            return StructDeclaration(className)
                    .WithInlineArrayAttribute(size)
                    .WithFileKeyword()
                    .WithTypeParameters("T")
                    .ExtendWithMembers(
                        CreateArg0Field(),
                        CreateConstructor(className, size)
                    );
        }

        private static ConstructorDeclarationSyntax CreateConstructor(string className, int size)
        {

            return ConstructorDeclaration(
                Identifier(className)
                )
                .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)
                    )
                )
                .ExtendWithParameters(
                    Enumerable.Range(0, size)
                    .Select(e => CreateParameter("T", $"value{e}"))
                    )
                .WithBody(
                Block(
                    GenerateAssignments(size)
                ));
        }
        private static IEnumerable<ExpressionStatementSyntax> GenerateAssignments(int size)
        {
            yield return Arg0Assignment();
            foreach (var index in Enumerable.Range(1, size - 1))
            {
                yield return ThisOfXAssignment(index);
            }
        }
        private static ExpressionStatementSyntax Arg0Assignment()
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("arg0"),
                    IdentifierName("value0")
                ));
        }

        private static ExpressionStatementSyntax ThisOfXAssignment(int i)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    ElementAccessExpression(
                        ThisExpression()
                        )
                    .WithArgumentList(
                        BracketedArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(i)
                                        )
                                    )
                                )
                            )
                        ),
                    IdentifierName($"value{i}")));
        }

        private static ParameterSyntax CreateParameter(string type, string name)
        {
            return Parameter(Identifier(name))
                .WithType(IdentifierName(type));
        }

        private static FieldDeclarationSyntax CreateArg0Field()
        {
            return FieldDeclaration(
                VariableDeclaration(IdentifierName("T"))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier("arg0")
                            )
                        )
                    )
                )
                .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)
                    )
                );
        }

        public static StructDeclarationSyntax WithInlineArrayAttribute(this StructDeclarationSyntax syntax, int size)
        {
            return syntax.WithAttributeLists(
                    SingletonList(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                AliasQualifiedName(
                                                    IdentifierName(
                                                        Token(SyntaxKind.GlobalKeyword)
                                                        ),
                                                    IdentifierName("System")
                                                    ),
                                                IdentifierName("Runtime")
                                                ),
                                            IdentifierName("CompilerServices")
                                            ),
                                        IdentifierName("InlineArray")
                                        )
                                    )
                                .WithArgumentList(
                                    AttributeArgumentList(
                                        SingletonSeparatedList<AttributeArgumentSyntax>(
                                            AttributeArgument(
                                                LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    Literal(size)
                                                    )
                                                )
                                            )
                                        )
                                    )
                            )
                        )
                    )
                );
        }

        public static StructDeclarationSyntax WithFileKeyword(this StructDeclarationSyntax syntax)
        {
            return syntax
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.FileKeyword)
                    )
                );
        }

    }
}
