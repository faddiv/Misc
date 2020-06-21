using System;
using System.Linq.Expressions;

namespace AngularParserCSharp
{
    internal static class ExpressionHelpers
    {
        #region  Public Methods

        public static Expression CastIfDifferentType(Expression expression, Type desiredType)
        {
            if (expression.Type != desiredType)
            {
                /*if (desiredType != typeof(object))
                {
                    if (!typeof(IConvertible).IsAssignableFrom(expression.Type))
                    {
                        throw new AngularException(
                            "Return type and expression type doesn't match and expression type can't be converted.");
                    }
                    if (!typeof(IConvertible).IsAssignableFrom(desiredType))
                        throw new AngularException(
                            "Return type and expression type doesn't match and return type can't be converted.");
                }*/
                expression = Expression.Convert(expression, desiredType);
            }
            return expression;
        }

        #endregion
    }
}