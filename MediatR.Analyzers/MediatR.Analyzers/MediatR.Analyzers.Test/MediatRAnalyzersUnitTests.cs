using Mediatr.Analyzers.Test.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = MediatR.Analyzers.Test.CSharpCodeFixVerifier<
    MediatR.Analyzers.MediatRRequestAnalyzer,
    MediatR.Analyzers.MediatRRequestCodeFixProvider>;

namespace MediatR.Analyzers.Test
{
    public class MediatRAnalyzersUnitTest
    {
        [Fact]
        public async Task Analysis_OnMissingHandler_ReportsHandlerMissing()
        {
            var test = CodeBuilder
                .WithDefaults()
                .AddCode(@"
public class {|#0:Message|} : IRequest<Response> { }
public class Response { }
");
            var expected = VerifyCS
                .Diagnostic(MediatRRequestAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("Message");

            //
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task Analysis_OnProvidedHandler_DoesntReportAnything()
        {
            var test = CodeBuilder
                .WithDefaults()
                .AddCode(@"
public class Message : IRequest<Response> { }
public class Response { }

public class MessageHandler : IRequestHandler<Message, Response>
{
    public Task<Response> Handle(Message request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}");

            await VerifyCS.VerifyAnalyzerNoDiagnosticResultAsync(test);
        }

        [Fact]
        public async Task Refactoring_WhenNoRequest1Handler_ShouldImplementIt()
        {
            var test = CodeBuilder
                .WithDefaults()
                .AddCode(@"
public class {|#0:Message|} : IRequest<Response> {
}
public class Response { }").ToString();

            var fixtest = CodeBuilder
                .WithDefaults()
                .AddCode(@"
public class Message : IRequest<Response> {
}

    public class MessageHandler : IRequestHandler<Message, Response>
    {
        public Task<Response> Handle(Message request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class Response { }").ToString();

            var expected = VerifyCS
               .Diagnostic(MediatRRequestAnalyzer.DiagnosticId)
               .WithLocation(0)
               .WithArguments("Message");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        [Fact]
        public async Task Refactoring_WhenNoRequestHandler_ShouldImplementIt()
        {
            var test = CodeBuilder
                .WithDefaults()
                .AddCode(@"
public class {|#0:Message|} : IRequest {
}
").ToString();

            var fixtest = CodeBuilder
                .WithDefaults()
                .AddCode(@"
public class Message : IRequest {
}

    public class MessageHandler : IRequestHandler<Message>
    {
        public Task Handle(Message request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }").ToString();

            var expected = VerifyCS
               .Diagnostic(MediatRRequestAnalyzer.DiagnosticId)
               .WithLocation(0)
               .WithArguments("Message");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
