using Microsoft.AspNetCore.Components;
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

        #region Leave
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

        public string Css { get; private set; }

        private Task EnterHandler(TransitionState state)
        {
            Css = EnteringCss;
            return OnEnter.InvokeAsync(state);
        }

        private Task EnteredHandler(TransitionState state)
        {
            Css = EnteredCss;
            return OnEntered.InvokeAsync(state);
        }

        private Task ExitHandler(TransitionState state)
        {
            Css = ExitingCss;
            return OnExiting.InvokeAsync(state);
        }

        private Task ExitedHandler(TransitionState state)
        {
            Css = ExitedCss;
            return OnExited.InvokeAsync(state);
        }
    }
}
