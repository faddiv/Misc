using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public class TimeoutEventExecutor
    {
        private readonly TransitionBase _parent;

        public bool Subscribed { get; private set; }

        public TimeoutEventExecutor(TransitionBase parent)
        {
            _parent = parent;
        }

        public async Task SubscribeTransitionEnd(ElementReference reference)
        {
            var transition = DotNetObjectReference.Create(_parent);
            if (_parent.JsRuntime is IJSInProcessRuntime jsRuntime2)
            {
                jsRuntime2.InvokeVoid("transitionEndHandler", reference, transition);
            }
            else
            {
                await _parent.JsRuntime.InvokeVoidAsync("transitionEndHandler", reference, transition);
            }
            Subscribed = true;
        }
    }
}
