using Blazorify.Client.Etc;
using Foxy.Blazor.Transition;
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
        public IReadOnlyDictionary<string, object> Attributes { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        private void OnEnter(TransitionState state)
        {
            _style = "height: 0px";
        }

        private async Task OnEntering(TransitionState state)
        {
            var height = await JsRuntime.InvokeAsync<int>("getElScrollSize", _reference.Ref);
            _style = $"height: {height}px";
        }
        private void OnEntered(TransitionState state)
        {
            _style = null;
        }

        private async Task OnExit(TransitionState state)
        {
            var height = await JsRuntime.InvokeAsync<int>("getElHeight", _reference.Ref);
            _style = $"height: {height}px";
        }

        private void OnExiting(TransitionState state)
        {
            _style = "height: 0px";
        }
        private void OnExited(TransitionState state)
        {
            _style = null;
        }

        private string Join(string css, IReadOnlyDictionary<string, object> attributes)
        {
            if (attributes.TryGetValue("class", out var css2)
                && css2 != null && !ReferenceEquals(css2, ""))
            {
                return string.Join(' ', css2, css);
            }
            return css;
        }
    }
}
