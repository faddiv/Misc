using System;
using System.Collections.Generic;

namespace Blazorify.Utilities.Styling
{
    public interface ICssBuilder
    {
        CssBuilder AddMultiple(params object[] values);

        CssBuilder Add(CssBuilder cssBuilder);

        CssBuilder Add(Enum enumValue);

        CssBuilder Add(IEnumerable<string> cssList);

        CssBuilder Add(IReadOnlyDictionary<string, object> attributes);

        CssBuilder Add(object values);

        CssBuilder Add(params (string, bool)[] tuple);

        CssBuilder Add(params (string, Func<bool>)[] tuple);

        CssBuilder Add(string value, bool condition = true);

        CssBuilder Add(string value, Func<bool> predicate);

        string ToString();
    }
}
