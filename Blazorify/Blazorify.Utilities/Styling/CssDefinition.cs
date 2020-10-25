using Blazorify.Utilities.Styling.Internals;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Blazorify.Utilities.Styling
{
    public class CssDefinition
    {
        private const string Separator = " ";
        private static readonly char[] _separatorArray = new[] { ' ' };

        private readonly List<string> _cssClasses;
        private readonly CssBuilderOptions _options;
        private readonly ThreadsafeCssBuilderCache _cache;

        internal CssDefinition(CssBuilderOptions options)
        {
            _cssClasses = new List<string>();
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (_options.EnumToClassNameConverter == null)
                throw new ArgumentException("Options.EnumToClassNameConverter can't be null.");
            if (_options.PropertyToClassNameConverter == null)
                throw new ArgumentException("Options.PropertyToClassNameConverter can't be null.");
            _cache = options.GetCache();
        }

        

        public override string ToString()
        {
            return string.Join(Separator, _cssClasses);
        }

        public CssDefinition AddMultiple(params object[] values)
        {
            if (values == null || values.Length == 0)
                return this;
            foreach (var value in values)
            {
                if (value is string strValue)
                {
                    AddInner(strValue);
                }
                else if (value is Enum enumValue)
                {
                    Add(enumValue);
                }
                else if (value is ValueTuple<string, bool> tupleWithCondition)
                {
                    AddInner(tupleWithCondition.Item1, tupleWithCondition.Item2);
                }
                else if (value is ValueTuple<string, Func<bool>> tupleWithPredicate)
                {
                    AddInner(tupleWithPredicate.Item1, tupleWithPredicate.Item2());
                }
                else if (value is IEnumerable<string> cssList)
                {
                    Add(cssList);
                }
                else if (value is CssDefinition other)
                {
                    Add(other);
                }
                else if (value is IReadOnlyDictionary<string, object> attributes)
                {
                    Add(attributes);
                }
                else
                {
                    Add(value);
                }
            }
            return this;
        }

        public CssDefinition Add(string cssClass, Func<bool> predicate)
        {
            AddInner(cssClass, predicate());
            return this;
        }

        public CssDefinition Add(string cssClass, bool condition = true)
        {
            AddInner(cssClass, condition);
            return this;
        }

        public CssDefinition Add(params (string, bool)[] tuple)
        {
            if (tuple == null || tuple.Length == 0)
                return this;
            foreach (var item in tuple)
            {
                AddInner(item.Item1, item.Item2);
            }
            return this;
        }

        public CssDefinition Add(params (string, Func<bool>)[] tuple)
        {
            if (tuple == null || tuple.Length == 0)
                return this;
            foreach (var item in tuple)
            {
                AddInner(item.Item1, item.Item2());
            }
            return this;
        }

        public CssDefinition Add(IEnumerable<string> cssList)
        {
            if (cssList == null)
                return this;
            foreach (var value in cssList)
            {
                AddInner(value);
            }
            return this;
        }

        public CssDefinition Add(CssDefinition cssBuilder)
        {
            if (cssBuilder == null)
                return this;
            foreach (var value in cssBuilder._cssClasses)
            {
                AddInner(value);
            }
            return this;
        }

        public CssDefinition Add(Enum enumValue)
        {
            if (enumValue == null)
                return this;
            var cssClass = _cache.GetOrAdd(enumValue,
                (ev) => _options.EnumToClassNameConverter.Invoke(ev));
            AddInner(cssClass);
            return this;
        }

        public CssDefinition Add(object values)
        {
            if (values == null)
                return this;

            var type = values.GetType();
            var extractor = _cache.GetOrAdd(type, CreateExtractor);
            extractor(values, AddInner);

            return this;
        }

        public CssDefinition Add(IReadOnlyDictionary<string, object> attributes)
        {
            if (attributes != null
                && attributes.TryGetValue("class", out var css)
                && css != null)
            {
                AddInner(css as string ?? css.ToString());
            }
            return this;
        }

        private ProcessCssDelegate CreateExtractor(Type type)
        {
            var lines = new List<Expression>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var valuesParam = Expression.Parameter(typeof(object));
            var addMethod = Expression.Parameter(typeof(AddCssDelegate));
            var valuesVar = Expression.Variable(type);
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
                var className = _options.PropertyToClassNameConverter(property);
                var classNameConstant = Expression.Constant(className);
                var invokation = Expression.Invoke(addMethod, classNameConstant, conditionGetter);
                lines.Add(invokation);
            }
            var body = Expression.Block(new ParameterExpression[] { valuesVar }, lines);
            var method = Expression.Lambda<ProcessCssDelegate>(body, valuesParam, addMethod);
            return method.Compile();
        }

        private void AddInner(string value, bool condition = true)
        {
            if (string.IsNullOrEmpty(value) || !condition)
                return;

            foreach (var cssClass in value.Split(_separatorArray, StringSplitOptions.RemoveEmptyEntries))
            {
                if (_options.ExcludeDuplication && _cssClasses.Contains(cssClass))
                    continue;
                _cssClasses.Add(cssClass);
            }
        }
    }

}
