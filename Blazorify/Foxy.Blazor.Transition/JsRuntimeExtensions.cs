using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public static class JsRuntimeExtensions
    {
        public static ValueTask<object> AddEventListener<TValue>(
            this IJSRuntime jsRuntime,
            ElementReference element,
            string eventName,
            DotNetObjectReference<TValue> target,
            string targetNethod,
            bool once)
            where TValue : class
        {
            return jsRuntime.InvokeAsync<object>(
                "Foxy.addBlazorEventListener",
                element,
                eventName,
                target,
                targetNethod,
                once);
        }
    }
}
