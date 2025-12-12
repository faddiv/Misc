using System.Reflection;

namespace ProxiesBenchmark.DispatchProxyExample
{
    public class DispatchProxyExampleInterceptor<T> : DispatchProxy where T : class
    {
        public DispatchProxyExampleInterceptor()
        {
        }

        public T Target { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                var result = targetMethod.Invoke(Target, args);

                return result;
            }
            catch (TargetInvocationException exc)
            {
                throw exc.InnerException;
            }
        }
    }
}
