using System.Linq.Expressions;

namespace AngularParserCSharp
{
    internal class TryGetInfo
    {
        #region Properties

        public ParameterExpression Variable { get; set; }
        public Expression VariableInit { get; set; }

        #endregion
    }
}