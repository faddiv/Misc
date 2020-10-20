using Blazorify.Client.Animate;
using Blazorify.Client.Etc;
using Blazorify.Utilities.Styling;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazorify.Client.Bootstrap
{
    partial class Collapse
    {
        private string _style = "";

        private DynamicTag _reference;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string Tag { get; set; } = "div";

        [Parameter]
        public bool Open { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        [Inject]
        public CssBuilderDelegate Css { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        private string GetCss(TransitionState state)
        {
            switch (state)
            {
                case TransitionState.Entering:
                    return "collapsing";
                case TransitionState.Entered:
                    return "collapse show";
                case TransitionState.Exiting:
                    return "collapsing";
                case TransitionState.Exited:
                default:
                    return "collapse";
            }
        }
        private void OnEnter(TransitionState state)
        {
        }

        private void OnEntering(TransitionState state)
        {
            _style = "height: 214px";
        }
        private void OnEntered(TransitionState state)
        {
            _style = "";
        }

        private void OnExit(TransitionState state)
        {
            _style = "height: 214px";
        }

        private async Task OnExiting(TransitionState state)
        {
            _style = "height: 0px";
        }
        private void OnExited(TransitionState state)
        {
            _style = "";
        }
    }
}
