using Microsoft.AspNetCore.Components;
using System;

namespace Foxy.Blazor.Transition
{
    partial class CssTransition
    {
        public static string DefaultAppearingCss = "appearing";
        public static string DefaultAppearedCss = "appeared";
        public static string DefaultEnteringCss = "entering";
        public static string DefaultEnteredCss = "entered";
        public static string DefaultExitingCss = "exiting";
        public static string DefaultExitedCss = "exited";

        #region Enter
        [Parameter]
        public string AppearingCss { get; set; } = DefaultAppearingCss;

        [Parameter]
        public string AppearedCss { get; set; } = DefaultAppearedCss;

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
        public RenderFragment<ICssTransitionRenderContext> ChildContent { get; set; }

        internal string GetCss(TransitionState state, bool appearing)
        {
            switch (state)
            {
                case TransitionState.Entering:
                    return appearing ? AppearingCss : EnteringCss;
                case TransitionState.Entered:
                    return appearing ? AppearedCss : EnteredCss;
                case TransitionState.Exiting:
                    return ExitingCss;
                case TransitionState.Exited:
                    return ExitedCss;
                default:
                    throw new Exception($"Invalid state in CssTransition: {state}");
            }
        }

        protected override CssTransitionContext CreateContext(TransitionType type, bool appearing)
        {
            return new CssTransitionContext(this, type, appearing);
        }
    }
}
