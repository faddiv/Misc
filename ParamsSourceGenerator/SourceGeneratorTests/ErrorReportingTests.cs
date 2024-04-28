using System.Threading.Tasks;
using Foxy.Params.SourceGenerator;
using Xunit;
using SourceGeneratorTests.TestInfrastructure;
using Foxy.Params.SourceGenerator.Data;
using Microsoft.CodeAnalysis;

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
            .WithArguments("Format", "object args");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task DoesntGenerateOnFaultyMethodParameter()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic("CS0246", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ThisIsWrong");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task DoesntGenerateOnFaultyReturnType()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic("CS0246", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("ThisIsWrong");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task Reports_OutReadOnlySpanParameter()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic(DiagnosticReports.OutModifierNotAllowedDescriptor)
            .WithLocation(0)
            .WithArguments("Format", "out System.ReadOnlySpan<object> args");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task DoesntGenerateOnNameDuplication()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic("CS0100", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("values");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task Reports_OnNameCollision()
    {
        string code = TestEnvironment.GetInvalidSource();

        var expected = VerifyCS
            .Diagnostic(DiagnosticReports.ParameterCollisionDescriptor)
            .WithLocation(0)
            .WithArguments("Format", "valuesSpan, values0, values1, values2");

        await VerifyCS.VerifyGeneratorAsync(code, expected, TestEnvironment.GetDefaultOuput());
    }
}
