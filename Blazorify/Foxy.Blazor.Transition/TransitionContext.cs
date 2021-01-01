using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public class TransitionContext :
        IEnterContext, IExitContext, ICalculationContext, ITransitionRenderContext, IDisposable
    {
        private TransitionState _state;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private int _transitionEndEventHandler;
        private ElementReference? _transitioningElement;
        private bool _disposed = false;

        public CancellationToken CancellationToken { get; }

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
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken = _cancellationTokenSource.Token;
        }

        public async Task SubscribeTransitionEnd(ElementReference reference)
        {
            _transitioningElement = reference;
            _transitionEndEventHandler = await JsRuntime.AddEventListenerAsync(
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
            if (_disposed)
                return;
            if (JsRuntime is IJSInProcessRuntime &&
                _transitionEndEventHandler != 0 &&
                _transitioningElement.HasValue)
            {
                JsRuntime.RemoveEventListener(_transitioningElement.Value, "transitionend", _transitionEndEventHandler);
            }
            JsReference.Dispose();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }

        internal void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();

            builder.Append(Type).Append(" ");
            if (Transitioning)
            {
                builder.Append("Transitioning ");
            } else
            {
                builder.Append("Stable ");
            }
            builder.Append(State);
            if(this.Appearing)
                builder.Append(" Appearing");
            if (In)
            {
                builder.Append(" In");
            }
            else
            {
                builder.Append(" Out");
            }
            if(Subscribed)
            {
                builder.Append(" Has transition end handler");
            }
            if(this._cancellationTokenSource.IsCancellationRequested)
            {
                builder.Append(" Cancelling");
            }
            return builder.ToString();
        }
    }
}
