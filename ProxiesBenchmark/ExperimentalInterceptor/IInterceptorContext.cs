using System.Reflection;

namespace ExperimentalInterceptor
{
    public interface IInterceptorContext<out TResult>
    {
        object Target { get; }

        int Length { get; }

        MethodInfo Method { get; }

        T Arg<T>(int index);

        void Update<T>(int index, T value);

        TResult Invoke();
    }
}
