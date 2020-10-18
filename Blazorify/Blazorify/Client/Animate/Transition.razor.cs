using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Blazorify.Client.Animate
{
    partial class Transition
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
        public RenderFragment<TransitionState> ChildContent { get; set; }

        [Parameter]
        public bool In { get; set; }

        [Parameter]
        public bool Appear { get; set; } = false;

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
                        await OnEnter.InvokeAsync(State);
                        State = TransitionState.Entering;
                        await Task.Yield();
                        await OnEntering.InvokeAsync(State);
                        await Task.Delay(EnterTimeout);
                    }
                }
                else
                {
                    if (ExitEnabled)
                    {
                        await OnExit.InvokeAsync(State);
                        State = TransitionState.Leaving;
                        await Task.Yield();
                        await OnExiting.InvokeAsync(State);
                        await Task.Delay(ExitTimeout);
                        _transitioning = false;
                    }
                }
                await TransitionedHandler();
                _transitioning = false;
            }
        }

        private async Task TransitionedHandler()
        {
            if (_innerState)
            {
                State = TransitionState.Entered;
                await OnEntered.InvokeAsync(State);
            }
            else
            {
                State = TransitionState.Leaved;
                await OnExited.InvokeAsync(State);
            }
        }
    }
}
