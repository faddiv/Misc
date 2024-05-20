using System.Threading.Tasks;
using Foxy.Params.SourceGenerator;
using Xunit;
using SourceGeneratorTests.TestInfrastructure;

namespace SourceGeneratorTests;

using VerifyCS = CSharpSourceGeneratorVerifier<ParamsIncrementalGenerator>;

public class SourceGenerationTests
{
    [Fact]
    public async Task Always_Generate_ParamsAttribute()
    {
        string code = @"";

        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetDefaultOuput());
    }

    [Fact]
    public async Task Generate_OverridesFor_ReadOnlySpan_WithDefaultParameters()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }


    [Fact]
    public async Task Generate_OverridesFor_CountedCase_WithMaxOverrides()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_OverridesFor_MultipleFixedParameters()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task DoesNotGenerateParams_WhenHasParamsIsFalse()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForInstanceLevelMethod()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForNonObjectReadOnlySpan()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForFunctions_WithKeywordReturnType()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForFunctions_WithNonKeywordReturnType()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithMultipleGenericFixedParameters()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithGenericParamsParameter()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericFunctions_WithGenericReturnType()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithRestrictedGenericParameters()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithRestrictedGenericParamsParameters()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForGenericMethods_WithRestrictedGenericReturnType()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForCustomTypeParametersAndReturnType()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForRefReadOnlySpan()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForRefReadonlyReadOnlySpan()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForInReadOnlySpan()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForSpecialFixedParams()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForRefReturn()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForRefReadonlyReturn()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForNullableParametersAndReturnType()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_ForDifferentReadOnlySpanName()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_WhenParametersDontCollide()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }

    [Fact]
    public async Task Generate_Overrides_WhenClassIsEmbedded()
    {
        string code = TestEnvironment.GetValidSource();
        await VerifyCS.VerifyGeneratorAsync(code,
            TestEnvironment.GetOuputs());
    }
}
