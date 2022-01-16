using System.Collections;
using System.Linq.Expressions;

namespace ActiveDirectoryPlayground
{
    public class PointlessLinqQueryable<T> : IQueryable<T>
    {
        internal readonly List<T> data;

        public PointlessLinqQueryable(List<T> data)
        {
            this.data = data;
            this.Provider = new PointlessLinqQueryProvider<T>(data);
            this.Expression = Expression.Constant(this);
        }

        public PointlessLinqQueryable(PointlessLinqQueryProvider<T> provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
