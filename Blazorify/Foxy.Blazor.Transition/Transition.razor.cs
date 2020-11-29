using Microsoft.AspNetCore.Components;

namespace Foxy.Blazor.Transition
{
    partial class Transition
    {
        [Parameter]
        public RenderFragment<ITransitionRenderContext> ChildContent { get; set; }

        protected override TransitionContext CreateContext(TransitionType type, bool appearing)
        {
            return new TransitionContext(this, type, appearing);
        }
    }
}
