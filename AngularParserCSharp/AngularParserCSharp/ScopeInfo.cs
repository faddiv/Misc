using System;
using System.Linq.Expressions;

namespace AngularParserCSharp
{
    internal class ScopeInfo
    {
        public ScopeInfo(Expression scopeExpression, bool assignScope = false)
        {
            ScopeExpression = scopeExpression;
            AssignScope = assignScope;
        }

        public ScopeInfo(ScopeInfo parent, bool? assignScope = null)
         : this(parent.ScopeExpression, assignScope ?? parent.AssignScope)
        {
        }
        /// <summary>
        /// Current scope need to work.
        /// </summary>
        public Expression ScopeExpression { get; }

        /// <summary>
        /// Type of the current scope.
        /// </summary>
        public Type Type => ScopeExpression?.Type;

        public bool IsJavascriptObject => Type == typeof(JavascriptObject);

        /// <summary>
        /// If true, then it is the left side of an assignment.
        /// </summary>
        public bool AssignScope { get; }
    }
}