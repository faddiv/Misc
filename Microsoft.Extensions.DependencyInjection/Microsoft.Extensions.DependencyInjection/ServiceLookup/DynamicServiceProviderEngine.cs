// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal sealed class DynamicServiceProviderEngine : CompiledServiceProviderEngine
    {
        private readonly ServiceProvider _serviceProvider;

        [RequiresDynamicCode("Creates DynamicMethods")]
        public DynamicServiceProviderEngine(ServiceProvider serviceProvider) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Func<ServiceProviderEngineScope, object?> RealizeService(ServiceCallSite callSite)
        {
            int callCount = 0;
            return scope =>
            {
                // Resolve the result before we increment the call count, this ensures that singletons
                // won't cause any side effects during the compilation of the resolve function.
                //var result = CallSiteRuntimeResolver.Instance.Resolve(callSite, scope);

                if (Interlocked.Increment(ref callCount) == 1)
                {
                    // Don't capture the ExecutionContext when forking to build the compiled version of the
                    // resolve function
                    void CallBack(object? _)
                    {
                        try
                        {
                            _serviceProvider.ReplaceServiceAccessor(callSite, base.RealizeService(callSite));
                        }
                        catch (Exception ex)
                        {
                            DependencyInjectionEventSource.Log.ServiceRealizationFailed(ex, _serviceProvider.GetHashCode());

                            Debug.Fail($"We should never get exceptions from the background compilation.{Environment.NewLine}{ex}");
                        }
                    }

                    CallBack(null);
                }
                var result = _serviceProvider.GetService(new ServiceIdentifier(callSite.Key, callSite.ServiceType), scope );
                    //CallSiteRuntimeResolver.Instance.Resolve(callSite, scope);
                return result;
            };
        }
    }
}
