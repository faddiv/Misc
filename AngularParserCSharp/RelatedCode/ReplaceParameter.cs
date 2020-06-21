using System.Linq.Expressions;

namespace OnlineWallet.Web.Common
{
    public class ReplaceParameter : ExpressionVisitor
    {
        #region Fields

        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        #endregion

        #region  Constructors

        public ReplaceParameter(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        #endregion

        #region  Nonpublic Methods

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (Equals(_source, node))
            {
                return _target;
            }

            return base.VisitParameter(node);
        }

        #endregion
    }
}