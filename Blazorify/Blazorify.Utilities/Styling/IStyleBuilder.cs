using System;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    public interface IStyleBuilder
    {
        StyleDefinition Add(IReadOnlyDictionary<string, object> attributes);

        StyleDefinition Add(string property, Func<string> value, bool condition = true);

        StyleDefinition Add(string property, Func<string> value, Func<bool> predicate);

        StyleDefinition Add(string property, string value, bool condition = true);

        StyleDefinition Add(string property, string value, Func<bool> predicate);

        StyleDefinition Add(StyleDefinition styleBuilder);

        StyleDefinition AddMultiple(params object[] values);

        string ToString();
    }
}
