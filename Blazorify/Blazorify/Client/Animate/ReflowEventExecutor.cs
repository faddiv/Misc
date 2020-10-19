using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazorify.Client.Animate
{
    public class ReflowEventExecutor
    {
        private readonly IJSRuntime _jsRuntime;

        public ReflowEventExecutor(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task Reflow(ElementReference reference)
        {
            if (_jsRuntime is IJSInProcessRuntime jsRuntime2)
            {
                jsRuntime2.InvokeVoid("reflow", reference);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("reflow", reference);
            }
        }
    }
}
