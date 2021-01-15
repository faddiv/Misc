using LightInject.Interception;
using System.Reflection;

namespace ProxiesBenchmark
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
