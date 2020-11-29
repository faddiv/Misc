namespace Foxy.Blazor.Transition
{
    public interface IEnterContext
    {
        TransitionState State { get; }

        bool Appearing { get; }
    }
}
