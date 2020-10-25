using System;
using System.Reflection;

namespace Blazorify.Utilities.Styling
{
    public class CssBuilderOptions
    {
        private readonly ThreadsafeCssBuilderCache _cache;

        public CssBuilderOptions()
        {
            _cache = new ThreadsafeCssBuilderCache();
        }

        public Func<PropertyInfo, string> PropertyToClassNameConverter { get; set; } = CssBuilderNamingConventions.KebabCaseWithUnderscoreToHyphen;

        public Func<Enum, string> EnumToClassNameConverter { get; set; } = CssBuilderNamingConventions.KebabCaseWithUnderscoreToHyphen;

        public bool ExcludeDuplication { get; set; } = false;

        internal ThreadsafeCssBuilderCache GetCache()
        {
            return _cache;
        }

        public void ClearCache()
        {
            _cache.ClearCache();
        }
    }
}
