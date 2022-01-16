using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace ActiveDirectoryPlayground
{
    public class PointlessLinqQueryProvider<T> : IQueryProvider
    {
        private List<T> data;

        public PointlessLinqQueryProvider(List<T> data)
        {
            this.data = data;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public object? Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var dataConst = Expression.Constant(data);
            if (expression is ConstantExpression constExp && constExp.Value is PointlessLinqQueryable<T> quer)
            {
                dataConst = Expression.Constant(quer.data);
            }
            if(expression is MethodCallExpression mcExp)
            {
                PointlessLinqQueryable<T> quer2 = new PointlessLinqQueryable<T>(data);
                if (mcExp.Method == GetMethodInfo(Queryable.FirstOrDefault, quer2))
                {
                    var call = Expression.Call(null, GetMethodInfo(Enumerable.FirstOrDefault, data), dataConst);
                    var lmb2 = Expression.Lambda<Func<TResult>>(call);
                    var fun2 = lmb2.Compile();
                    return fun2();
                }
            }
            var convert = Expression.Convert(dataConst, typeof(TResult));
            var lmb = Expression.Lambda<Func<TResult>>(convert);
            var fun = lmb.Compile();
            return fun();
        }

        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6)
        {
            return f.Method;
        }
    }
}
