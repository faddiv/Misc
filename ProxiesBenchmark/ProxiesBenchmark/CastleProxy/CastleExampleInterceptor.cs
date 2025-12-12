using System.Reflection;
using Castle.DynamicProxy;

namespace ProxiesBenchmark.CastleProxy
{
    public class CastleExampleInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
