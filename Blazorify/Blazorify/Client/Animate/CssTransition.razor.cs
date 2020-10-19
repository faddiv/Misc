using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Blazorify.Client.Animate
{
    partial class CssTransition
    {
        public static string DefaultEnteringCss = "entering";
        public static string DefaultEnteredCss = "entered";
        public static string DefaultExitingCss = "exiting";
        public static string DefaultExitedCss = "exited";

        #region Enter
        [Parameter]
        public bool EnterEnabled { get; set; } = true;

        [Parameter]
        public int EnterTimeout { get; set; }

        [Parameter]
        public string EnteringCss { get; set; } = DefaultEnteringCss;

        [Parameter]
        public string EnteredCss { get; set; } = DefaultEnteredCss;

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
        public string ExitingCss { get; set; } = DefaultExitingCss;

        [Parameter]
        public string ExitedCss { get; set; } = DefaultExitedCss;

        [Parameter]
        public EventCallback<TransitionState> OnExit { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnExiting { get; set; }

        [Parameter]
        public EventCallback<TransitionState> OnExited { get; set; }
        #endregion

        [Parameter]
        public RenderFragment<CssTransitionContext> ChildContent { get; set; }

        [Parameter]
        public bool In { get; set; }

        [Parameter]
        public bool Appear { get; set; } = false;

        [Parameter]
        public EventCallback<ReflowEventExecutor> OnReflow { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        private async Task EnteringHandler(TransitionState state)
        {
            await Reflow();
            await OnEntering.InvokeAsync(state);
        }

        private async Task ExitingHandler(TransitionState state)
        {
            await Reflow();
            await OnExiting.InvokeAsync(state);
        }

        private string GetCss(TransitionState state)
        {
            return state switch
            {
                TransitionState.Entering => EnteringCss,
                TransitionState.Entered => EnteredCss,
                TransitionState.Exiting => ExitingCss,
                TransitionState.Exited => ExitedCss,
                _ => throw new Exception($"Invalid state in CssTransition: {state}"),
            };
        }

        private async Task Reflow()
        {
            var evt = new ReflowEventExecutor(JsRuntime);
            await OnReflow.InvokeAsync(evt);
        }
    }
}
