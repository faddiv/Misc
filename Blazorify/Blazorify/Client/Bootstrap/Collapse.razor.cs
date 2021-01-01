using Blazorify.Client.Etc;
using Foxy.Blazor.Transition;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
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
        public bool In { get; set; }

        [Parameter]
        public string Tag { get; set; } = "div";

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        public void Toggle()
        {
            In = !In;
        }

        private void OnEnter(IEnterContext state)
        {
            _style = "height: 0px";
        }

        private async Task OnEntering(IEnterContext state)
        {
            var height = await JsRuntime.InvokeAsync<int>("getElScrollSize", _reference.Ref);
            _style = $"height: {height}px";
        }

        private void OnEntered(IEnterContext state)
        {
            _style = null;
        }

        private async Task OnExit(IExitContext state)
        {
            var height = await JsRuntime.InvokeAsync<double>("getElHeight", _reference.Ref);
            _style = FormattableString.Invariant($"height: {height:#.##}px");
        }

        private void OnExiting(IExitContext state)
        {
            _style = "height: 0px";
        }
        private void OnExited(IExitContext state)
        {
            _style = null;
        }

        private string Join(string css, IReadOnlyDictionary<string, object> attributes)
        {
            if (attributes != null
                && attributes.TryGetValue("class", out var css2) == true
                && css2 != null && !ReferenceEquals(css2, ""))
            {
                return string.Join(' ', css2, css);
            }
            return css;
        }
    }
}
