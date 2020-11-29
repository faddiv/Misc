using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public class TransitionBase : ComponentBase
    {
        private bool _transitioning = false;
        private bool _internalIn = false;

        [Inject]
        internal IJSRuntime JsRuntime { get; set; }

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
        public EventCallback<bool> InChanged { get; set; }


        [Parameter]
        public bool Appear { get; set; } = false;

        public TransitionState State { get; private set; }

        public async Task Show()
        {
            if (_internalIn)
                return;
            if (_transitioning)
                return;
            _transitioning = true;
            _internalIn = true;

            if (EnterEnabled)
            {
                var executor = OnCalculateEnd.HasDelegate
                    ? new TimeoutEventExecutor(this)
                    : null;

                await FireEnter();
                State = TransitionState.Entering;
                if (executor != null)
                {
                    await OnCalculateEnd.InvokeAsync(executor);
                }
                await Task.Yield();
                await FireEntering();
                if (executor?.Subscribed == true)
                    return;
                await Task.Delay(EnterTimeout);
            }
            await TransitionedHandler();
        }

        public async Task Hide()
        {
            if (!_internalIn)
                return;
            if (_transitioning)
                return;
            _transitioning = true;
            _internalIn = false;

            if (ExitEnabled)
            {
                var executor = OnCalculateEnd.HasDelegate
                    ? new TimeoutEventExecutor(this)
                    : null;

                await FireExit();
                State = TransitionState.Exiting;
                if (executor != null)
                {
                    await OnCalculateEnd.InvokeAsync(executor);
                }
                await Task.Yield();
                await FireExiting();
                if (executor?.Subscribed == true)
                    return;
                await Task.Delay(ExitTimeout);
            }
            await TransitionedHandler();
        }

        public Task Toggle()
        {
            if (In)
            {
                return Hide();
            } else
            {
                return Show();
            }
        }

        protected override Task OnInitializedAsync()
        {
            if (In)
            {
                State = TransitionState.Entered;
            }
            else
            {
                State = TransitionState.Exited;
            }
            _internalIn = In;
            return base.OnInitializedAsync();
        }

        protected override Task OnParametersSetAsync()
        {
            if (_internalIn != In)
            {
                if (In)
                {
                    return Show();
                }
                else
                {
                    return Hide();
                }
            }
            return base.OnParametersSetAsync();
        }

        protected virtual async Task FireEnter()
        {
            await OnEnter.InvokeAsync(State);
        }

        protected virtual async Task FireEntering()
        {
            await OnEntering.InvokeAsync(State);
        }

        protected virtual async Task FireEntered()
        {
            await OnEntered.InvokeAsync(State);
        }

        protected virtual async Task FireExit()
        {
            await OnExit.InvokeAsync(State);
        }

        protected virtual async Task FireExiting()
        {
            await OnExiting.InvokeAsync(State);
        }

        protected virtual async Task FireExited()
        {
            await OnExited.InvokeAsync(State);
        }

        [JSInvokable]
        public async Task TransitionedHandler()
        {
            if (_internalIn)
            {
                State = TransitionState.Entered;
            }
            else
            {
                State = TransitionState.Exited;
            }

            if(In != _internalIn)
            {
                In = _internalIn;
                await InChanged.InvokeAsync(In);
            }
            _transitioning = false;

            if (_internalIn)
            {
                await FireEntered();
            }
            else
            {
                await FireExited();
            }
        }

    }
}
