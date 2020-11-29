namespace Foxy.Blazor.Transition
{
    public interface ITransitionRenderContext
    {
        TransitionState State { get; }

        bool Appearing { get; }
    }


    public interface ICssTransitionRenderContext : ITransitionRenderContext
    {
        string Css { get; }
    }
}
