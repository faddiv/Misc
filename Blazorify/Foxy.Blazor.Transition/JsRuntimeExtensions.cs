using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public static class JsRuntimeExtensions
    {
        public static ValueTask<int> AddEventListenerAsync<TValue>(
            this IJSRuntime jsRuntime,
            ElementReference element,
            string eventName,
            DotNetObjectReference<TValue> target,
            string targetNethod,
            bool once)
            where TValue : class
        {
            return jsRuntime.InvokeAsync<int>(
                "Foxy.addBlazorEventListener",
                element,
                eventName,
                target,
                targetNethod,
                once);
        }

        public static ValueTask<int> AddEventListener<TValue>(
            this IJSRuntime jsRuntime,
            ElementReference element,
            string eventName,
            DotNetObjectReference<TValue> target,
            string targetNethod,
            bool once)
            where TValue : class
        {
            if (jsRuntime is IJSInProcessRuntime jsClient)
            {
                return jsClient.InvokeAsync<int>(
                    "Foxy.addBlazorEventListener",
                    element,
                    eventName,
                    target,
                    targetNethod,
                    once);
            }
            else
            {
                throw NotSupported();
            }
        }

        public static ValueTask RemoveEventListenerAsync(
            this IJSRuntime jsRuntime,
            ElementReference element,
            string eventName,
            int functionHandler)
        {
            return jsRuntime.InvokeVoidAsync(
                "Foxy.removeBlazorEventListener",
                element,
                eventName,
                functionHandler);
        }

        public static void RemoveEventListener(
            this IJSRuntime jsRuntime,
            ElementReference element,
            string eventName,
            int functionHandler)
        {
            if(jsRuntime is IJSInProcessRuntime jsClient)
            {
                jsClient.InvokeVoid(
                "Foxy.removeBlazorEventListener",
                element,
                eventName,
                functionHandler);
            } else
            {
                throw NotSupported();
            }
        }

        private static NotSupportedException NotSupported()
        {
            return new NotSupportedException("Synchronous javascript call not supported on server side.");
        }
    }
}
