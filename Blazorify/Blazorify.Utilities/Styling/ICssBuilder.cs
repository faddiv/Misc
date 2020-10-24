using System;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    public interface ICssBuilder
    {
        CssDefinition AddMultiple(params object[] values);

        CssDefinition Add(CssDefinition cssBuilder);

        CssDefinition Add(Enum enumValue);

        CssDefinition Add(IEnumerable<string> cssList);

        CssDefinition Add(IReadOnlyDictionary<string, object> attributes);

        CssDefinition Add(object values);

        CssDefinition Add(params (string, bool)[] tuple);

        CssDefinition Add(params (string, Func<bool>)[] tuple);

        CssDefinition Add(string value, bool condition = true);

        CssDefinition Add(string value, Func<bool> predicate);

        string ToString();
    }
}
