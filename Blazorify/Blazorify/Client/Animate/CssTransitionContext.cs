namespace Blazorify.Client.Animate
{
    public class CssTransitionContext
    {
        public CssTransitionContext(string css, TransitionState state)
        {
            Css = css;
            State = state;
        }

        public string Css { get; }

        public TransitionState State { get; set; }
    }
}
