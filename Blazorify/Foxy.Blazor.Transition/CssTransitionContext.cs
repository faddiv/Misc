namespace Foxy.Blazor.Transition
{
    public class CssTransitionContext : TransitionContext, ICssTransitionRenderContext
    {
        public string Css { get; private set; }
        public CssTransitionContext(
            ITransitionBase parent,
            TransitionType type,
            bool appearing) :
            base(parent, type, appearing)
        {
        }

        protected override void StateChanged()
        {
            var parent = (CssTransition)Parent;
            Css = parent.GetCss(State, Appearing);
        }
    }
}
