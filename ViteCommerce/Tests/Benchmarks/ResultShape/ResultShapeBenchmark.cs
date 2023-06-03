using BenchmarkDotNet.Attributes;
using ViteCommerce.Api.Common.DomainAbstractions;

namespace Benchmarks.ResultShape;

[MemoryDiagnoser]
public class ResultShapeBenchmark
{
    private readonly Dummy _value = new();
        private readonly Exception _ex = new Exception("Error");
    /*
    [Benchmark]
    public string CastV1_Ok()
    {
        var ok = new DomainResponseV1<Dummy>(_value);
        return ok
            .OnSuccess(e => e.Value)
            .OnFail(e => e.Message)
            .OnEmpty(() => "None")
            .ToValue();
    }

    [Benchmark]
    public string CastV1_Error()
    {
        var ok = new DomainResponseV1<Dummy>(_ex);
        return ok
            .OnSuccess(e => e.Value)
            .OnFail(e => e.Message)
            .OnEmpty(() => "None")
            .ToValue();
    }

    [Benchmark]
    public string CastV1_Empty()
    {
        var ok = new DomainResponseV1<Dummy>();
        return ok
            .OnSuccess(e => e.Value)
            .OnFail(e => e.Message)
            .OnEmpty(() => "None")
            .ToValue();
    }
    */
    /*[Benchmark]
    public string CastV2_Ok()
    {
        var ok = new DomainResponseV2<Dummy>(_value);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastV2_Error()
    {
        var ok = new DomainResponseV2<Dummy>(_ex);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastV2_Empty()
    {
        var ok = new DomainResponseV2<Dummy>();
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }*/
    /*
    [Benchmark]
    public string CastV3_Ok()
    {
        var ok = new DomainResponseV3<Dummy>(_value);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastV3_Error()
    {
        var ok = new DomainResponseV3<Dummy>(_ex);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastV3_Empty()
    {
        var ok = new DomainResponseV3<Dummy>();
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }*/
    [Benchmark]
    public string CastV4_Ok()
    {
        var ok = new DomainResponseV4<Dummy>(_value);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastV4_Error()
    {
        var ok = new DomainResponseV4<Dummy>(_ex);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastV4_Empty()
    {
        var ok = new DomainResponseV4<Dummy>();
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastClass_Ok()
    {
        ClassDomainResponse<Dummy> ok = new OkDomainResponse<Dummy>(_value);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastClass_Error()
    {
        ClassDomainResponse<Dummy> ok = new ValidationFailedDomainResponse<Dummy>(_ex);
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }

    [Benchmark]
    public string CastClass_Empty()
    {
        ClassDomainResponse<Dummy> ok = NotFoundDomainResponse<Dummy>.Instance;
        return ok
            .Match(
                onSuccess: e => e.Value,
                onFail: e => e.Message,
                onEmpty: () => "None");
    }
}
