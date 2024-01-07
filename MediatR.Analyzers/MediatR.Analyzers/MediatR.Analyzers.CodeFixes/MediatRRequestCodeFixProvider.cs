using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MediatR.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MediatRRequestCodeFixProvider)), Shared]
    public class MediatRRequestCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(MediatRRequestAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            var responseType = diagnostic.Properties["Response"];

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Generate Handler",
                    createChangedDocument: c => AddHandler(context.Document, declaration, responseType, c),
                    equivalenceKey: "Generate Handler"),
                diagnostic);
        }

        private async Task<Document> AddHandler(Document document, TypeDeclarationSyntax typeDecl, string responseType, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var identifierToken = typeDecl.Identifier;
            var handler = responseType is null
                ? CreateHandler1(identifierToken.Text + "Handler", identifierToken.Text)
                : CreateHandler2(identifierToken.Text + "Handler", identifierToken.Text, responseType);

            editor.InsertAfter(typeDecl, new[] {
                handler
            });
            return editor.GetChangedDocument();
        }

        /*
         * public class MessageHandler : IRequestHandler<Message, Response>
         * {
         *     public Task<Response> Handle(Message request, CancellationToken cancellationToken)
         *     {
         *         throw new NotImplementedException();
         *     }
         *}
         * */
        private static SyntaxNode CreateHandler1(string name, string messageType)
        {
            return
            ClassDeclaration(name)
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)))
            .WithBaseList(
                BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(
                            GenericName(
                                Identifier("IRequestHandler"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(messageType))))))))
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    MethodDeclaration(
                        IdentifierName("Task"),
                        Identifier("Handle"))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]{
                                    Parameter(
                                        Identifier("request"))
                                    .WithType(
                                        IdentifierName(messageType)),
                                    Token(SyntaxKind.CommaToken),
                                    Parameter(
                                        Identifier("cancellationToken"))
                                    .WithType(
                                        IdentifierName("CancellationToken"))})))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ThrowStatement(
                                    ObjectCreationExpression(
                                        IdentifierName("NotImplementedException"))
                                    .WithArgumentList(
                                        ArgumentList())))))));
        }

        /*
         * public class MessageHandler : IRequestHandler<Message>
         * {
         *     public Task Handle(Message request, CancellationToken cancellationToken)
         *     {
         *         throw new NotImplementedException();
         *     }
         *}
         * */
        private SyntaxNode CreateHandler2(string name, string messageType, string responseType)
        {
            return
                        ClassDeclaration(name)
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword)))
                        .WithBaseList(
                            BaseList(
                                SingletonSeparatedList<BaseTypeSyntax>(
                                    SimpleBaseType(
                                        GenericName(
                                            Identifier("IRequestHandler"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]{
                                                        IdentifierName(messageType),
                                                        Token(SyntaxKind.CommaToken),
                                                        IdentifierName(responseType)})))))))
                        .WithMembers(
                            SingletonList<MemberDeclarationSyntax>(
                                MethodDeclaration(
                                    GenericName(
                                        Identifier("Task"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName(responseType)))),
                                    Identifier("Handle"))
                                .WithModifiers(
                                    TokenList(
                                        Token(SyntaxKind.PublicKeyword)))
                                .WithParameterList(
                                    ParameterList(
                                        SeparatedList<ParameterSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Parameter(
                                                    Identifier("request"))
                                                .WithType(
                                                    IdentifierName(messageType)),
                                                Token(SyntaxKind.CommaToken),
                                                Parameter(
                                                    Identifier("cancellationToken"))
                                                .WithType(
                                                    IdentifierName("CancellationToken"))})))
                                .WithBody(
                                    Block(
                                        SingletonList<StatementSyntax>(
                                            ThrowStatement(
                                                ObjectCreationExpression(
                                                    IdentifierName("NotImplementedException"))
                                                .WithArgumentList(
                                                    ArgumentList())))))));
        }
    }
}
