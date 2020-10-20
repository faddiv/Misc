using System;
using System.Reflection;

namespace Blazorify.Utilities.Styling
{
    public interface ICssBuilderNamingConvention
    {
        string ToCssClassName(PropertyInfo property);

        string ToCssClassName(Enum enumValue);
    }
}
