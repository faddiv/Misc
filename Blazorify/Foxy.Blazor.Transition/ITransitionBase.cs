using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public interface ITransitionBase
    {
        IJSRuntime JSRuntime { get; }

        Task TransitionedHandler(TransitionContext context);
    }
}
