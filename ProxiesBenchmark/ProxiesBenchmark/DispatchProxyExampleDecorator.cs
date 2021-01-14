using System.Reflection;

namespace ProxiesBenchmark
{
    public class DispatchProxyExampleDecorator<T> : DispatchProxy where T : class
    {
        public DispatchProxyExampleDecorator()
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
