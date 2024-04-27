using System.Threading.Tasks;
using Foxy.Params.SourceGenerator;
using Xunit;
using Xunit.Abstractions;
using SourceGeneratorTests.TestInfrastructure;

namespace SourceGeneratorTests;

using VerifyCS = CSharpSourceGeneratorVerifier<ParamsIncrementalGenerator>;

public class Tests
{
    [Fact]
    public async Task Always_Generate_ParamsAttribute()
    {
        string code = @"";

        await VerifyCS.VerifyGeneratorAsync(code, ("ParamsAttribute.g.cs", TestEnvironment.AttributeImpl));
    }

    [Fact]
    public async Task Generate_OverridesFor_ReadOnlySpan_WithDefaultParameters()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }


    [Fact]
    public async Task Generate_OverridesFor_CountedCase_WithMaxOverrides()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_OverridesFor_MultipleFixedParameters()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task DoesNotGenerateParams_WhenHasParamsIsFalse()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForInstanceLevelMethod()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForNonObjectReadOnlySpan()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForFunctions_WithKeywordReturnType()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForFunctions_WithNonKeywordReturnType()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithMultipleGenericFixedParameters()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithGenericParamsParameter()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericFunctions_WithGenericReturnType()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithRestrictedGenericParameters()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithRestrictedGenericParamsParameters()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithRestrictedGenericReturnType()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForCustomTypeParametersAndReturnType()
    {
        string code = TestEnvironment.GetSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }
}
