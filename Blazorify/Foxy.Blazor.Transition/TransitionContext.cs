using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public class TransitionContext :
        IEnterContext, IExitContext, ICalculationContext, ITransitionRenderContext, IDisposable
    {
        private TransitionState _state;

        internal bool Subscribed { get; private set; }

        protected ITransitionBase Parent { get; }

        public TransitionType Type { get; }

        protected IJSRuntime JsRuntime => Parent.JSRuntime;

        protected DotNetObjectReference<TransitionContext> JsReference { get; }

        public TransitionState State
        {
            get => _state;
            internal set
            {
                if (_state != value)
                {
                    _state = value;
                    StateChanged();
                }
            }
        }

        public bool Transitioning { get; internal set; }

        public bool In { get; internal set; }

        public bool Appearing { get; }

        public TransitionContext(
            ITransitionBase parent,
            TransitionType type,
            bool appearing)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Type = type;
            Appearing = appearing;
            Transitioning = true;
            JsReference = DotNetObjectReference.Create(this);
        }

        public async Task SubscribeTransitionEnd(ElementReference reference)
        {
            await JsRuntime.AddEventListener(
                reference, "transitionend",
                JsReference, nameof(TransitionedHandler),
                true);
            Subscribed = true;
        }

        [JSInvokable]
        public void TransitionedHandler()
        {
            Parent.TransitionedHandler(this);
        }

        protected virtual void StateChanged()
        {

        }

        public void Dispose()
        {
            JsReference.Dispose();
        }
    }

    public class CssTransitionContext : TransitionContext, ICssTransitionRenderContext
    {
        public string Css { get; private set; }
        public CssTransitionContext(
            ITransitionBase parent,
            TransitionType type,
            bool appearing) :
            base(parent, type, appearing)
        {
        }

        protected override void StateChanged()
        {
            var parent = (CssTransition)Parent;
            Css = parent.GetCss(State, Appearing);
        }
    }
}
