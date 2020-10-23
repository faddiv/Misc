using System;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    public interface IStyleBuilder
    {
        StyleBuilder Add(IReadOnlyDictionary<string, object> attributes);

        StyleBuilder Add(string property, Func<string> value, bool condition = true);

        StyleBuilder Add(string property, Func<string> value, Func<bool> predicate);

        StyleBuilder Add(string property, string value, bool condition = true);

        StyleBuilder Add(string property, string value, Func<bool> predicate);

        StyleBuilder Add(StyleBuilder styleBuilder);

        StyleBuilder AddMultiple(params object[] values);

        string ToString();
    }
}
