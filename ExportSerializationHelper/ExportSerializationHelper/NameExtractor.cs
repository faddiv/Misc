using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExportSerializationHelper
{
    internal class NameExtractor : ExpressionVisitor
    {
        public LinkedList<string> ExtractedNames { get; } = new LinkedList<string>();
        protected override Expression VisitMember(MemberExpression node)
        {
            ExtractedNames.AddFirst(node.Member.Name);
            return base.VisitMember(node);
        }
    }
}
