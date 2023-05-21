using BenchmarkDotNet.Attributes;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ViteCommerce.Api.Application.MediatrSolution;
using ViteCommerce.Api.Application.SelfContained;
using ViteCommerce.Api.Application.Subclassing;
using ViteCommerce.Api.Configurations;

namespace ViteCommerce.Benchmarks;

[MemoryDiagnoser]
public class BenchmarkForMediators
{
    private Mediator.IMediator _mediator;
    private MediatR.IMediator _mediatr;
    private IServiceProvider sp;
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediatorWithPipelines();
        services.AddMediatrWithPipelines();
        services.AddScoped<IValidator<ValidatedGet>, ValidatedGetValidator>();
        services.AddScoped<IValidator<ValidatedGet2>, ValidatedGetValidator2>();
        services.AddScoped<IValidator<ValidatedGet3>, ValidatedGetValidator3>();
        sp = services.BuildServiceProvider();
        _mediator = sp.GetRequiredService<Mediator.IMediator>();
        _mediatr = sp.GetRequiredService<MediatR.IMediator>();
    }

    ValidatedGet2 model1 = new ValidatedGet2("asdf");
    [Benchmark]
    public Task<IResult> GeneratedMediator_Validated_Ok()
    {
        //using var scope = sp.CreateScope();
        //var _mediator = scope.ServiceProvider.GetRequiredService<Mediator.IMediator>();
        return SubclassingApi.SubClassingValidated(model1, _mediator);
    }

    ValidatedGet2 model2 = new ValidatedGet2("");
    [Benchmark]
    public Task<IResult> GeneratedMediator_Validated_ValidationFailed()
    {
        //using var scope = sp.CreateScope();
        //var _mediator = scope.ServiceProvider.GetRequiredService<Mediator.IMediator>();
        
        return SubclassingApi.SubClassingValidated(model2, _mediator);
    }

    ValidatedGet2 model3 = new ValidatedGet2("NoContent");
    [Benchmark]
    public Task<IResult> GeneratedMediator_ValidatedNoContent()
    {
        //using var scope = sp.CreateScope();
        //var _mediator = scope.ServiceProvider.GetRequiredService<Mediator.IMediator>();
        return SubclassingApi.SubClassingValidated(model3, _mediator);
    }

    UnvalidatedGet2 model4 = new UnvalidatedGet2("asdf");
    [Benchmark]
    public Task<IResult> GeneratedMediator_Unvalidated_Ok()
    {
        //using var scope = sp.CreateScope();
        //var _mediator = scope.ServiceProvider.GetRequiredService<Mediator.IMediator>();
        
        return SubclassingApi.SubClassingUnvalidated(model4, _mediator);
    }

    ValidatedGet3 model5 = new ValidatedGet3("asdf");
    [Benchmark]
    public Task<IResult> MediatR_Validated_Ok()
    {
        //using var scope = sp.CreateScope();
        //var _mediatr = sp.GetRequiredService<MediatR.IMediator>();
        
        return MediatrSolutionApi.MediatrValidated(model5, _mediatr);
    }

    ValidatedGet3 model6 = new ValidatedGet3("");
    [Benchmark]
    public Task<IResult> MediatR_Validated_ValidationFailed()
    {
        //using var scope = sp.CreateScope();
        //var _mediatr = sp.GetRequiredService<MediatR.IMediator>();
        
        return MediatrSolutionApi.MediatrValidated(model6, _mediatr);
    }

    ValidatedGet3 model7 = new ValidatedGet3("NoContent");
    [Benchmark]
    public Task<IResult> MediatR_ValidatedNoContent()
    {
        //using var scope = sp.CreateScope();
        //var _mediatr = sp.GetRequiredService<MediatR.IMediator>();
        return MediatrSolutionApi.MediatrValidated(model7, _mediatr);
    }

    UnvalidatedGet3 model8 = new UnvalidatedGet3("asdf");
    [Benchmark]
    public Task<IResult> MediatR_Unvalidated_Ok()
    {
        //using var scope = sp.CreateScope();
        //var _mediatr = sp.GetRequiredService<MediatR.IMediator>();
        return MediatrSolutionApi.MediatrUnvalidated(model8, _mediatr);
    }
}
