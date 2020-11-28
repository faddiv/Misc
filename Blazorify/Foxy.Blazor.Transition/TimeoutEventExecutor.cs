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
            await _parent.JsRuntime.AddEnetListener(reference, "transitionend", transition, "TransitionedHandler", true);
            Subscribed = true;
        }
    }
}
