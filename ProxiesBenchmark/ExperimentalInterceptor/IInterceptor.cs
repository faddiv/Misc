namespace ExperimentalInterceptor
{
    public interface IInterceptor
    {
        TResult Invoke<TContext, TResult>(TContext context)
            where TContext : IInterceptorContext<TResult>;
    }
}
