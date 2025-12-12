using System.Reflection;
using LightInject.Interception;

namespace ProxiesBenchmark.LightInjectExample
{
    internal class LightInjectInterceptor : IInterceptor
    {
        public object Invoke(IInvocationInfo invocationInfo)
        {
            try
            {
                return invocationInfo.Proceed();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
