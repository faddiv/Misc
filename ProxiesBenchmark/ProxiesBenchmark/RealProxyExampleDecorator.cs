using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ProxiesBenchmark
{
    public class RealProxyExampleDecorator<T> : RealProxy where T : MarshalByRefObject
    {
        private readonly T _target;
        private static object[] empty = new object[0];

        public RealProxyExampleDecorator(T target)
            : base(target.GetType())
        {
            _target = target;
        }
        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IMethodCallMessage methodCallMsg)
            {
                try
                {
                    var args = methodCallMsg.Args;
                    var result = methodCallMsg.MethodBase.Invoke(_target, args);
                    return new ReturnMessage(result, empty, empty.Length, methodCallMsg.LogicalCallContext, methodCallMsg);
                }
                catch (TargetInvocationException ex)
                {
                    return new ReturnMessage(ex.InnerException, methodCallMsg);
                }
            }
            throw new ArgumentException($"Invalid message; expected IMethodCallMessage but {msg?.GetType().FullName ?? "null"} recieved.", nameof(msg));
        }
    }
}
