using System.Threading.Tasks;
using Foxy.Params.SourceGenerator;
using Xunit;
using SourceGeneratorTests.TestInfrastructure;

namespace SourceGeneratorTests;

using VerifyCS = CSharpSourceGeneratorVerifier<ParamsIncrementalGenerator>;

public class ErrorReportingTests
{
    [Fact]
    public async Task Reports_NoPartialKeyword()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic(DiagnosticReports.PartialIsMissingDescriptor)
            .WithLocation(0)
            .WithArguments("Foo", "Format");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task Reports_NoParameter()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic(DiagnosticReports.ParameterMissingDescriptor)
            .WithLocation(0)
            .WithArguments("Format");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task Reports_NonReadOnlySpanParameter()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic(DiagnosticReports.ParameterMismatchDescriptor)
            .WithLocation(0)
            .WithArguments("Format", "object");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }
}
