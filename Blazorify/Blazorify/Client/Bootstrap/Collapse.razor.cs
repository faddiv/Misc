using Blazorify.Client.Animate;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazorify.Client.Bootstrap
{
    partial class Collapse
    {
        private string style = "";

        private ElementReference reference;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public bool Open { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string,object> Attributes { get; set; }


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
                case TransitionState.Leaving:
                    return "collapsing";
                case TransitionState.Leaved:
                default:
                    return "collapse";
            }
        }
        private void OnEnter(TransitionState state)
        {
        }

        private void OnEntering(TransitionState state)
        {
            style = "height: 214px";
        }
        private void OnEntered(TransitionState state)
        {
            style = "";
        }

        private async Task OnExit(TransitionState state)
        {
            style = "height: 214px";
            if (JsRuntime is IJSInProcessRuntime jsRuntime2)
            {
                jsRuntime2.InvokeVoid("reflow", reference);
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("reflow", reference);
            }
        }

        private async Task OnExiting(TransitionState state)
        {
            
            style = "height: 0px";
        }
        private void OnExited(TransitionState state)
        {
            style = "";
        }
    }
}
