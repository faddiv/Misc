using System;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    public class StyleBuilder : IStyleBuilder
    {

        private readonly List<StyleElement> _styles;

        public static IStyleBuilder Create()
        {
            return new StyleBuilder();
        }

        public StyleBuilder()
        {
            _styles = new List<StyleElement>();
        }

        public StyleBuilder Add(string property, string value, bool condition = true)
        {
            AddInner(property, value, condition);
            return this;
        }

        public StyleBuilder Add(string property, Func<string> value, bool condition = true)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            AddInner(property, value(), condition);
            return this;
        }

        public StyleBuilder Add(string property, string value, Func<bool> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            AddInner(property, value, predicate());
            return this;
        }

        public StyleBuilder Add(string property, Func<string> value, Func<bool> predicate)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            AddInner(property, value(), predicate());
            return this;
        }

        public StyleBuilder Add(StyleBuilder styleBuilder)
        {
            if (styleBuilder is null)
                return this;

            foreach (var item in styleBuilder._styles)
            {
                AddInner(item.Property, item.Value);
            }
            return this;
        }

        public StyleBuilder Add(IReadOnlyDictionary<string, object> attributes)
        {
            if (attributes is null)
                return this;

            if (attributes.TryGetValue("style", out var style)
                && !(style is null))
            {
                var styleStr = (style as string) ?? style.ToString();
                if (styleStr.Length == 0)
                    return this;
                var stylePairs = styleStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var stylePairStr in stylePairs)
                {
                    var stylePair = stylePairStr.Split(new[] { ':' });
                    if (stylePair.Length != 2)
                    {
                        throw new Exception($"Invalid style found in the attributes.style: '{styleStr}'");
                    }
                    AddInner(stylePair[0], stylePair[1]);
                }
            }
            return this;
        }

        public StyleBuilder AddMultiple(params object[] values)
        {
            if (values == null || values.Length == 0)
                return this;
            foreach (var item in values)
            {
                if (item == null)
                    continue;
                if (item is ValueTuple<string, string> type1)
                {
                    AddInner(type1.Item1, type1.Item2);
                }
                else if (item is ValueTuple<string, Func<string>> type2)
                {
                    AddInner(type2.Item1, type2.Item2());
                }
                else if (item is ValueTuple<string, string, bool> type3)
                {
                    AddInner(type3.Item1, type3.Item2, type3.Item3);
                }
                else if (item is ValueTuple<string, Func<string>, bool> type4)
                {
                    AddInner(type4.Item1, type4.Item2(), type4.Item3);
                }
                else if (item is ValueTuple<string, string, Func<bool>> type5)
                {
                    AddInner(type5.Item1, type5.Item2, type5.Item3());
                }
                else if (item is ValueTuple<string, Func<string>, Func<bool>> type6)
                {
                    AddInner(type6.Item1, type6.Item2(), type6.Item3());
                }
                else if (item is StyleBuilder styleBuilder)
                {
                    Add(styleBuilder);
                }
                else if (item is IReadOnlyDictionary<string, object> attributes)
                {
                    Add(attributes);
                }
            }
            return this;
        }

        public override string ToString()
        {
            return string.Join(";", _styles);
        }

        private void AddInner(string property, string value, bool condition = true)
        {
            ValidateProperty(property);
            if (condition && !string.IsNullOrEmpty(value))
            {
                _styles.Add(new StyleElement(property, value));
            }
        }

        private static void ValidateProperty(string property)
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentException($"'{nameof(property)}' cannot be null or empty", nameof(property));
        }

    }
}