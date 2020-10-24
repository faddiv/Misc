using System;

namespace Blazorify.Utilities.Styling
{
    public class CssBuilder : ICssBuilder
    {
        private readonly CssBuilderOptions _options;

        public CssBuilder(CssBuilderOptions options)
        {
            _options = options;
        }

        public CssDefinition Create(CssBuilderOptions options = null)
        {
            return new CssDefinition(options ?? _options);
        }

        public CssDefinition this[params object[] values]
        {
            get
            {
                return Create().AddMultiple(values);
            }
        }

        public CssDefinition this[string cssClass, params (string, Func<bool>)[] tuple]
        {
            get
            {
                return Create().Add(cssClass).Add(tuple);
            }
        }
    }
}
