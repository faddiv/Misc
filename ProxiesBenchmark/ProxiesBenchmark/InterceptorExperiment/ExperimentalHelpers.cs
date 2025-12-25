namespace ProxiesBenchmark.InterceptorExperiment
{
    public class ExperimentalHelpers
    {
        public static ICalculator WithExperimental(Calculator target)
        {
            var interceptor = new ExperimentalInterceptor();
            return new SourceGeneratedProxy(target, interceptor);
        }
    }
}
