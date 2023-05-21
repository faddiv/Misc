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
public class BenchmarkForPipelines
{
    private MediatR.IMediator _mediatr1;
    private MediatR.IMediator _mediatr2;
    private IServiceProvider sp1;
    private IServiceProvider sp2;
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatrServiceCollectionExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(Api.PipelineBehaviors.ValidationBehaviorMediatr<,>));
        });
        services.AddScoped<IValidator<ValidatedGet>, ValidatedGetValidator>();
        services.AddScoped<IValidator<ValidatedGet2>, ValidatedGetValidator2>();
        services.AddScoped<IValidator<ValidatedGet3>, ValidatedGetValidator3>();
        sp1 = services.BuildServiceProvider();
        _mediatr1 = sp1.GetRequiredService<MediatR.IMediator>();

        services = new ServiceCollection();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatrServiceCollectionExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(Api.PipelineBehaviors.ValidationBehaviorMediatr2<,>));
        });
        services.AddScoped<IValidator<ValidatedGet>, ValidatedGetValidator>();
        services.AddScoped<IValidator<ValidatedGet2>, ValidatedGetValidator2>();
        services.AddScoped<IValidator<ValidatedGet3>, ValidatedGetValidator3>();
        sp2 = services.BuildServiceProvider();
        _mediatr2 = sp2.GetRequiredService<MediatR.IMediator>();
    }

    ValidatedGet3 model1 = new ValidatedGet3("asdf");
    [Benchmark]
    public Task<IResult> Validated__PipelineV1()
    {
        return MediatrSolutionApi.MediatrValidated(model1, _mediatr1);
    }

    UnvalidatedGet3 model2 = new UnvalidatedGet3("asdf");
    [Benchmark]
    public Task<IResult> Unvalidated_PipelineV1()
    {
        return MediatrSolutionApi.MediatrUnvalidated(model2, _mediatr1);
    }

    ValidatedGet3 model3 = new ValidatedGet3("asdf");
    [Benchmark]
    public Task<IResult> Validated_PipelineV2()
    {
        return MediatrSolutionApi.MediatrValidated(model3, _mediatr2);
    }

    UnvalidatedGet3 model4 = new UnvalidatedGet3("asdf");
    [Benchmark]
    public Task<IResult> Unvalidated_PipelineV2()
    {
        return MediatrSolutionApi.MediatrUnvalidated(model4, _mediatr2);
    }
}
