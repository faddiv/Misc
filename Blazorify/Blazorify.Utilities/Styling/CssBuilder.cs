using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Blazorify.Utilities.Styling
{
    public class CssBuilder
    {
        private delegate void AddDelegate(string name, bool condition);

        private const string Separator = " ";

        public List<string> Values { get; }

        private static readonly Dictionary<Type, Action<object, AddDelegate>> _valueExtractors = new Dictionary<Type, Action<object, AddDelegate>>();
        public CssBuilder()
        {
            Values = new List<string>();
        }

        public override string ToString()
        {
            return string.Join(Separator, Values);
        }
        public static CssBuilder Create(params object[] values)
        {
            var builder = new CssBuilder();
            if (values == null || values.Length == 0)
                return builder;
            foreach (var value in values)
            {
                if (value is string strValue)
                {
                    builder.AddInner(strValue);
                }
                else if (value is ValueTuple<string, bool> tupleWithCondition)
                {
                    builder.AddInner(tupleWithCondition.Item1, tupleWithCondition.Item2);
                }
                else if (value is ValueTuple<string, Func<bool>> tupleWithPredicate)
                {
                    builder.AddInner(tupleWithPredicate.Item1, tupleWithPredicate.Item2);
                }
                else if (value is IEnumerable<string> cssList)
                {
                    builder.Add(cssList);
                }
                else if (value is CssBuilder other)
                {
                    builder.Add(other);
                }
                else if (value is IReadOnlyDictionary<string, object> attributes)
                {
                    builder.Add(attributes);
                }
                else
                {
                    builder.Add(value);
                }
            }
            return builder;
        }

        public CssBuilder Add(string value, Func<bool> predicate)
        {
            AddInner(value, predicate);
            return this;
        }

        public CssBuilder Add(string value, bool condition = true)
        {
            AddInner(value, condition);
            return this;
        }

        public CssBuilder Add(params (string, bool)[] tuple)
        {
            if (tuple == null || tuple.Length == 0)
                return this;
            foreach (var item in tuple)
            {
                AddInner(item.Item1, item.Item2);
            }
            return this;
        }

        public CssBuilder Add(params (string, Func<bool>)[] tuple)
        {
            if (tuple == null || tuple.Length == 0)
                return this;
            foreach (var item in tuple)
            {
                AddInner(item.Item1, item.Item2);
            }
            return this;
        }

        public CssBuilder Add(IEnumerable<string> cssList)
        {
            if (cssList == null)
                return this;
            Values.AddRange(cssList);
            return this;
        }

        public CssBuilder Add(CssBuilder cssBuilder)
        {
            if (cssBuilder == null)
                return this;
            Values.AddRange(cssBuilder.Values);
            return this;
        }

        public CssBuilder Add(object values)
        {
            if (values != null)
            {
                var type = values.GetType();
                if (!_valueExtractors.TryGetValue(type, out var extractor))
                {
                    extractor = CreateExtractor(type);
                    _valueExtractors.Add(type, extractor);
                }
                extractor(values, AddInner);
            }
            return this;
        }

        public CssBuilder Add(IReadOnlyDictionary<string, object> attributes)
        {
            if (attributes != null
                && attributes.TryGetValue("class", out var css)
                && css != null)
            {
                AddInner(css as string ?? css.ToString());
            }
            return this;
        }

        private Action<object, AddDelegate> CreateExtractor(Type type)
        {
            var lines = new List<Expression>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var valuesParam = Expression.Parameter(typeof(object));
            var addMethod = Expression.Parameter(typeof(AddDelegate));
            var valuesVar = Expression.Variable(type, "target");
            var castedValuesParam = Expression.Convert(valuesParam, type);
            var valuesVarAssigment = Expression.Assign(valuesVar, castedValuesParam);
            lines.Add(valuesVarAssigment);
            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(bool))
                {
                    throw new Exception($"Only boolean properties allowed for the css builder. Invalid poperty: {type.Name}.{property.Name} (Type: {property.PropertyType}");
                }
                var conditionGetter = Expression.Property(valuesVar, property);
                var className = GenerateName(property);
                var classNameConstant = Expression.Constant(className);
                var invokation = Expression.Invoke(addMethod, classNameConstant, conditionGetter);
                lines.Add(invokation);
            }
            var body = Expression.Block(new ParameterExpression[] { valuesVar }, lines);
            var method = Expression.Lambda<Action<object, AddDelegate>>(body, valuesParam, addMethod);
            return method.Compile();
        }

        private string GenerateName(PropertyInfo property)
        {
            var propertyName = property.Name;
            if (propertyName.StartsWith('@'))
            {
                propertyName = propertyName.Substring(1);
            }
            return propertyName.Replace('_', '-');
        }

        private void AddInner(string value, Func<bool> predicate)
        {
            AddInner(value, predicate());
        }

        private void AddInner(string value, bool condition = true)
        {
            if (!string.IsNullOrEmpty(value) && condition)
            {
                Values.Add(value);
            }
        }

    }
}
