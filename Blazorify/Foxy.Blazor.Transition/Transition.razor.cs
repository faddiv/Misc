using Microsoft.AspNetCore.Components;

namespace Foxy.Blazor.Transition
{
    partial class Transition
    {
        [Parameter]
        public RenderFragment<TransitionState> ChildContent { get; set; }

    }
}
