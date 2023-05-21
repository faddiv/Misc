using BenchmarkDotNet.Attributes;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ViteCommerce.Api.Application.SelfContained;
using ViteCommerce.Api.Application.Subclassing;
using ViteCommerce.Api.Configurations;

namespace ViteCommerce.Benchmarks;

[MemoryDiagnoser]
public class BenchmarkForDomainResponses
{
    private IMediator _mediator;
    private IServiceProvider sp;
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediatorWithPipelines();
        services.AddScoped<IValidator<ValidatedGet>, ValidatedGetValidator>();
        services.AddScoped<IValidator<ValidatedGet2>, ValidatedGetValidator2>();
        sp = services.BuildServiceProvider();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public Task<IResult> SelfContained_Validated_Ok()
    {
        return SelfContainedApi.SelfContainedValidated(new ValidatedGet("asdf"), _mediator);
    }

    [Benchmark]
    public Task<IResult> SelfContained_Validated_ValidationFailed()
    {
        return SelfContainedApi.SelfContainedValidated(new ValidatedGet(""), _mediator);
    }

    [Benchmark]
    public Task<IResult> SelfContained_ValidatedNoContent()
    {
        return SelfContainedApi.SelfContainedValidated(new ValidatedGet("NoContent"), _mediator);
    }

    [Benchmark]
    public Task<IResult> SelfContained_Unvalidated_Ok()
    {
        return SelfContainedApi.SelfContainedUnvalidated(new UnvalidatedGet("asdf"), _mediator);
    }

    [Benchmark]
    public Task<IResult> SubClassing_Validated_Ok()
    {
        return SubclassingApi.SubClassingValidated(new ValidatedGet2("asdf"), _mediator);
    }

    [Benchmark]
    public Task<IResult> SubClassing_Validated_ValidationFailed()
    {
        return SubclassingApi.SubClassingValidated(new ValidatedGet2(""), _mediator);
    }

    [Benchmark]
    public Task<IResult> SubClassing_ValidatedNoContent()
    {
        return SubclassingApi.SubClassingValidated(new ValidatedGet2("NoContent"), _mediator);
    }

    [Benchmark]
    public Task<IResult> SubClassing_Unvalidated_Ok()
    {
        return SubclassingApi.SubClassingUnvalidated(new UnvalidatedGet2("asdf"), _mediator);
    }

}
