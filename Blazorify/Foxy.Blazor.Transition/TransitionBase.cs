using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
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
        }

        [Inject]
        public ILogger<TransitionBase<TContext>> Logger { get; set; }

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
            var context = Context;
            try
            {
                Logger.LogInformation("Show started {0}", context);
                if (!context.Transitioning
                    && context.State == TransitionState.Entered)
                {
                    return;
                }
                if (context.Transitioning)
                {
                    Logger.LogInformation("Cancel");
                    context.Cancel();
                }

                context = CreateContextAndMakeCurrentInternal(TransitionType.Enter, appearing);
                context.State = TransitionState.Exited;
                var token = context.CancellationToken;

                if (EnterEnabled)
                {
                    Logger.LogInformation("FireEnter started {0}", context);
                    await FireEnter(context);
                    token.ThrowIfCancellationRequested();
                    context.State = TransitionState.Entering;
                    Logger.LogInformation("OnCalculateEnd started {0}", context);
                    await OnCalculateEnd.InvokeAsync(context);
                    token.ThrowIfCancellationRequested();
                    Logger.LogInformation("Yield started {0}", context);
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                    Logger.LogInformation("FireEntering started {0}", context);
                    await FireEntering(context);
                    token.ThrowIfCancellationRequested();
                    Logger.LogInformation("Wait for end {0}", context);
                    if (context.Subscribed == true)
                        return;
                    await Task.Delay(EnterTimeout);
                    token.ThrowIfCancellationRequested();
                }
                await TransitionedHandler(context);
            }
            catch (Exception ex)
            when (ex is OperationCanceledException || ex is InvalidOperationException)
            {
                Logger.LogInformation("Cancel executed {0}", context);
            }
        }

        public async Task Hide()
        {
            var context = Context;
            try
            {
                Logger.LogInformation("Hide started {0}", context);
                if (!context.Transitioning
                    && context.State == TransitionState.Exited)
                {
                    return;
                }
                if (context.Transitioning)
                {
                    Logger.LogInformation("Cancel");
                    context.Cancel();
                }

                context = CreateContextAndMakeCurrentInternal(TransitionType.Exit, false);
                context.State = TransitionState.Entered;
                var token = context.CancellationToken;

                if (ExitEnabled)
                {
                    Logger.LogInformation("FireExit started {0}", context);
                    await FireExit(context);
                    token.ThrowIfCancellationRequested();
                    context.State = TransitionState.Exiting;
                    Logger.LogInformation("OnCalculateEnd started {0}", context);
                    await OnCalculateEnd.InvokeAsync(context);
                    token.ThrowIfCancellationRequested();
                    Logger.LogInformation("Yield started {0}", context);
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                    Logger.LogInformation("FireExiting started {0}", context);
                    await FireExiting(context);
                    token.ThrowIfCancellationRequested();
                    Logger.LogInformation("Wait for end {0}", context);
                    if (context.Subscribed == true)
                        return;
                    await Task.Delay(ExitTimeout);
                    token.ThrowIfCancellationRequested();
                }
                await TransitionedHandler(context);
            }
            catch (Exception ex)
            when (ex is OperationCanceledException || ex is InvalidOperationException)
            {
                Logger.LogInformation("Cancel executed {0}", context);
            }
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
                    var context = CreateContextAndMakeCurrentInternal(TransitionType.Enter, false);
                    context.State = TransitionState.Entered;
                    context.Transitioning = false;
                }
            }
            else
            {
                var context = CreateContextAndMakeCurrentInternal(TransitionType.Exit, false);
                context.State = TransitionState.Exited;
                context.Transitioning = false;
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
            Logger.LogInformation("In changed {0} vs {1}", In, Context);
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
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            try
            {
                Logger.LogInformation("End handler started {0}", context);
                var token = context.CancellationToken;
                token.ThrowIfCancellationRequested();
                switch (context.Type)
                {
                    case TransitionType.Enter:
                        context.State = TransitionState.Entered;
                        context.In = true;
                        context.Transitioning = false;

                        if (context.In != In)
                        {
                            Logger.LogInformation("SetIn {0}", context);
                            await SetIn(context.In);
                            token.ThrowIfCancellationRequested();
                        }
                        Logger.LogInformation("FireEntered {0}", context);
                        await FireEntered(context);
                        break;
                    case TransitionType.Exit:
                        context.State = TransitionState.Exited;
                        context.In = false;
                        context.Transitioning = false;

                        if (context.In != In)
                        {
                            Logger.LogInformation("SetIn {0}", context);
                            await SetIn(context.In);
                            token.ThrowIfCancellationRequested();
                        }
                        Logger.LogInformation("FireExited {0}", context);
                        await FireExited(context);
                        break;
                    default:
                        throw new InvalidOperationException($"the given state is not enter nor exit.");
                }
                context.Dispose();
            }
            catch (Exception ex)
            when (ex is OperationCanceledException || ex is InvalidOperationException)
            {
                Logger.LogInformation("Cancel executed {0}", context);
            }
        }

        private TContext CreateContextAndMakeCurrentInternal(TransitionType type, bool appearing)
        {
            var context = Context;
            if (context != null)
                context.Dispose();
            context = CreateContext(type, appearing);
            Context = context;
            return context;
        }

        protected abstract TContext CreateContext(TransitionType type, bool appearing);

        public void Dispose()
        {
            Context?.Dispose();
        }

        private async Task SetIn(bool newIn)
        {
            In = newIn;
            await InChanged.InvokeAsync(newIn);
        }

    }
}
