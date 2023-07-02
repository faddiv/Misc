using Mediatr.Analyzers.Test.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = MediatR.Analyzers.Test.CSharpCodeFixVerifier<
    MediatR.Analyzers.MediatRRequestAnalyzer,
    MediatR.Analyzers.MediatRAnalyzersCodeFixProvider>;

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
        //Diagnostic and CodeFix both triggered and checked for
        //[Fact]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("MediatrAnalyzers").WithLocation(0).WithArguments("TypeName");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
