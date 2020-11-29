using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Foxy.Blazor.Transition
{
    public interface ICalculationContext
    {
        Task SubscribeTransitionEnd(ElementReference reference);
    }
}
