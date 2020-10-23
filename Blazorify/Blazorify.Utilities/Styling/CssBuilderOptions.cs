using System;
using System.Reflection;

namespace Blazorify.Utilities.Styling
{
    public class CssBuilderOptions
    {
        public Func<PropertyInfo, string> PropertyToClassNameConverter { get; set; } = CssBuilderNamingConventions.KebabCaseWithUnderscoreToHyphen;

        public Func<Enum, string> EnumToClassNameConverter { get; set; } = CssBuilderNamingConventions.KebabCaseWithUnderscoreToHyphen;
        
    }
}
