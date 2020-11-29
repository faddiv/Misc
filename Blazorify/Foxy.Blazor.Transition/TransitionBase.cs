using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public abstract class TransitionBase<TContext> : ComponentBase, ITransitionBase, IDisposable
        where TContext : TransitionContext
    {
        public TransitionBase()
        {
            JsReference = DotNetObjectReference.Create<ComponentBase>(this);
        }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        #region Enter
        [Parameter]
        public bool EnterEnabled { get; set; } = true;

        [Parameter]
        public int EnterTimeout { get; set; }

        [Parameter]
        public EventCallback<IEnterContext> OnEnter { get; set; }

        [Parameter]
        public EventCallback<IEnterContext> OnEntering { get; set; }

        [Parameter]
        public EventCallback<IEnterContext> OnEntered { get; set; }
        #endregion

        #region Exit
        [Parameter]
        public bool ExitEnabled { get; set; } = true;

        [Parameter]
        public int ExitTimeout { get; set; }

        [Parameter]
        public EventCallback<IExitContext> OnExit { get; set; }

        [Parameter]
        public EventCallback<IExitContext> OnExiting { get; set; }

        [Parameter]
        public EventCallback<IExitContext> OnExited { get; set; }
        #endregion

        [Parameter]
        public EventCallback<ICalculationContext> OnCalculateEnd { get; set; }

        [Parameter]
        public bool In { get; set; }

        [Parameter]
        public EventCallback<bool> InChanged { get; set; }

        public TContext Context { get; private set; }

        public TransitionState State => Context?.State ?? (In ? TransitionState.Entered : TransitionState.Exited);

        [Parameter]
        public bool Appear { get; set; } = false;

        public DotNetObjectReference<ComponentBase> JsReference { get; }

        public async Task Toggle()
        {
            if (In)
            {
                await Hide();
            }
            else
            {
                await Show(false);
            }
        }

        public async Task Show(bool appearing)
        {
            if (Context.Transitioning)
            {
                // TODO Cancel
                return;
            }
            if (Context.State == TransitionState.Entered)
            {
                return;
            }

            var context = CreateContextInternal(TransitionType.Enter, appearing);
            context.State = TransitionState.Exited;

            if (EnterEnabled)
            {
                await FireEnter(context);
                context.State = TransitionState.Entering;
                await OnCalculateEnd.InvokeAsync(context);
                await Task.Yield();
                await FireEntering(context);
                if (context.Subscribed == true)
                    return;
                await Task.Delay(EnterTimeout);
            }
            await TransitionedHandler(context);
        }

        public async Task Hide()
        {
            if (Context.Transitioning)
            {
                // TODO Cancel
                return;
            }
            if(Context.State == TransitionState.Exited)
            {
                return;
            }
            var context = CreateContextInternal(TransitionType.Exit, false);
            context.State = TransitionState.Entered;

            if (ExitEnabled)
            {
                await FireExit(context);
                context.State = TransitionState.Exiting;
                await OnCalculateEnd.InvokeAsync(context);
                await Task.Yield();
                await FireExiting(context);
                if (context.Subscribed == true)
                    return;
                await Task.Delay(ExitTimeout);
            }
            await TransitionedHandler(context);
        }

        protected override Task OnInitializedAsync()
        {
            if (In)
            {
                if (Appear)
                {
                    return Show(true);
                }
                else
                {
                    Context = CreateContextInternal(TransitionType.Enter, false);
                    Context.State = TransitionState.Entered;
                    Context.Transitioning = false;
                }
            }
            else
            {
                Context = CreateContextInternal(TransitionType.Exit, false);
                Context.State = TransitionState.Exited;
                Context.Transitioning = false;
            }
            return base.OnInitializedAsync();
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            var inChanged = parameters.TryGetValue(nameof(In), out bool newIn);
            if (In == newIn)
            {
                inChanged = false;
            }
            await base.SetParametersAsync(parameters);
            if (!inChanged)
            {
                return;
            }
            if (Context.In == In)
            {
                return;
            }
            if (In)
            {
                await Show(false);
            }
            else
            {
                await Hide();
            }
        }

        protected virtual async Task FireEnter(IEnterContext context)
        {
            await OnEnter.InvokeAsync(context);
        }

        protected virtual async Task FireEntering(IEnterContext context)
        {
            await OnEntering.InvokeAsync(context);
        }

        protected virtual async Task FireEntered(IEnterContext context)
        {
            await OnEntered.InvokeAsync(context);
        }

        protected virtual async Task FireExit(IExitContext context)
        {
            await OnExit.InvokeAsync(context);
        }

        protected virtual async Task FireExiting(IExitContext context)
        {
            await OnExiting.InvokeAsync(context);
        }

        protected virtual async Task FireExited(IExitContext context)
        {
            await OnExited.InvokeAsync(context);
        }

        [JSInvokable]
        public async Task TransitionedHandler(TransitionContext context)
        {
            switch (context.Type)
            {
                case TransitionType.Enter:
                    context.State = TransitionState.Entered;
                    context.In = true;
                    context.Transitioning = false;

                    if (context.In != In)
                    {
                        await SetIn(context.In);
                    }
                    await FireEntered(context);
                    break;
                case TransitionType.Exit:
                    context.State = TransitionState.Exited;
                    context.In = false;
                    context.Transitioning = false;

                    if (context.In != In)
                    {
                        await SetIn(context.In);
                    }
                    await FireExited(context);
                    break;
                default:
                    throw new InvalidOperationException($"the given state is not enter nor exit.");
            }
        }

        private TContext CreateContextInternal(TransitionType type, bool appearing)
        {
            if (Context != null)
                Context.Dispose();
            var context = CreateContext(type, appearing);
            Context = context;
            return context;
        }

        protected abstract TContext CreateContext(TransitionType type, bool appearing);

        public void Dispose()
        {
            JsReference.Dispose();
        }

        private async Task SetIn(bool newIn)
        {
            In = newIn;
            await InChanged.InvokeAsync(newIn);
        }

    }
}
