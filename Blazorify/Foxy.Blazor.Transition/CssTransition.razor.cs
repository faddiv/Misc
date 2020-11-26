using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;

namespace Foxy.Blazor.Transition
{
    partial class CssTransition
    {
        public static string DefaultEnteringCss = "entering";
        public static string DefaultEnteredCss = "entered";
        public static string DefaultExitingCss = "exiting";
        public static string DefaultExitedCss = "exited";

        #region Enter
        [Parameter]
        public string EnteringCss { get; set; } = DefaultEnteringCss;

        [Parameter]
        public string EnteredCss { get; set; } = DefaultEnteredCss;
        #endregion

        #region Exit
        [Parameter]
        public string ExitingCss { get; set; } = DefaultExitingCss;

        [Parameter]
        public string ExitedCss { get; set; } = DefaultExitedCss;
        #endregion

        [Parameter]
        public RenderFragment<string> ChildContent { get; set; }

        private string GetCss(TransitionState state)
        {
            switch (state)
            {
                case TransitionState.Entering:
                    return EnteringCss;
                case TransitionState.Entered:
                    return EnteredCss;
                case TransitionState.Exiting:
                    return ExitingCss;
                case TransitionState.Exited:
                    return ExitedCss;
                default:
                    throw new Exception($"Invalid state in CssTransition: {state}");
            }
        }
    }
}
