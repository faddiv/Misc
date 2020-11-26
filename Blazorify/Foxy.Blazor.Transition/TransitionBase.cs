using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public class TransitionBase : ComponentBase
    {
        private bool _transitioning = false;
        private bool _innerState;

        #region Enter
        [Parameter]
        public bool EnterEnabled { get; set; } = true;

        [Parameter]
        public int EnterTimeout { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnEnter { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnEntering { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnEntered { get; set; }
        #endregion

        #region Exit
        [Parameter]
        public bool ExitEnabled { get; set; } = true;

        [Parameter]
        public int ExitTimeout { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnExit { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnExiting { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnExited { get; set; }
        #endregion

        [Parameter]
        public EventCallback<TimeoutEventExecutor> OnCalculateEnd { get; set; }

        [Parameter]
        public bool In { get; set; }

        [Parameter]
        public bool Appear { get; set; } = false;

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        public TransitionState State { get; private set; }

        protected override Task OnInitializedAsync()
        {
            if (Appear)
            {
                _innerState = !In;
                return TransitioningHandle();
            }
            else
            {
                _innerState = In;
                return TransitionedHandler();
            }
        }

        protected override Task OnParametersSetAsync()
        {
            return TransitioningHandle();
        }

        protected virtual async Task FireEnter()
        {
            await OnEnter.InvokeAsync(State);
        }

        private async Task FireEntering()
        {
            await OnEntering.InvokeAsync(State);
        }

        protected async Task FireEntered()
        {
            await OnEntered.InvokeAsync(State);
        }

        private async Task FireExit()
        {
            await OnExit.InvokeAsync(State);
        }

        private async Task FireExiting()
        {
            await OnExiting.InvokeAsync(State);
        }

        protected async Task FireExited()
        {
            await OnExited.InvokeAsync(State);
        }

        private async Task TransitioningHandle()
        {

            while (_innerState != In)
            {
                if (_transitioning)
                {
                    return;
                }
                _transitioning = true;
                _innerState = In;
                if (_innerState)
                {
                    if (EnterEnabled)
                    {
                        await FireEnter();
                        var executor = new TimeoutEventExecutor(this);
                        await OnCalculateEnd.InvokeAsync(executor);
                        State = TransitionState.Entering;
                        await Task.Yield();
                        await FireEntering();
                        if (executor.Subscribed)
                            return;
                        await Task.Delay(EnterTimeout);
                    }
                }
                else
                {
                    if (ExitEnabled)
                    {
                        await FireExit();
                        var executor = new TimeoutEventExecutor(this);
                        await OnCalculateEnd.InvokeAsync(executor);
                        State = TransitionState.Exiting;
                        await Task.Yield();
                        await FireExiting();
                        if (executor.Subscribed)
                            return;
                        await Task.Delay(ExitTimeout);
                    }
                }
                await TransitionedHandler();
            }
        }

        [JSInvokable]
        public async Task TransitionedHandler()
        {
            if (_innerState)
            {
                State = TransitionState.Entered;
                await FireEntered();
            }
            else
            {
                State = TransitionState.Exited;
                await FireExited();
            }
            _transitioning = false;
        }

    }
}
