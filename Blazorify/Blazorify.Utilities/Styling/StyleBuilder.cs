using System;

namespace Blazorify.Utilities.Styling
{
    public class StyleBuilder : IStyleBuilder
    {
        private readonly ThreadsafeStyleBuilderCache _cache;

        public StyleBuilder()
        {
            _cache = new ThreadsafeStyleBuilderCache();
        }

        public StyleDefinition Create()
        {
            return new StyleDefinition(_cache);
        }

        public StyleDefinition this[params object[] arguments]
        {
            get
            {
                return Create().AddMultiple(arguments);
            }
        }

        public StyleDefinition this[params (string, string, Func<bool>)[] arguments]
        {
            get
            {
                var style = Create();
                foreach (var item in arguments)
                {
                    style.Add(item.Item1, item.Item2, item.Item3);
                }
                return style;
            }
        }

        public StyleDefinition this[params (string, Func<string>, bool)[] arguments]
        {
            get
            {
                var style = Create();
                foreach (var item in arguments)
                {
                    style.Add(item.Item1, item.Item2, item.Item3);
                }
                return style;
            }
        }

        public StyleDefinition this[params (string, Func<string>, Func<bool>)[] arguments]
        {
            get
            {
                var style = Create();
                foreach (var item in arguments)
                {
                    style.Add(item.Item1, item.Item2, item.Item3);
                }
                return style;
            }
        }

        public void ClearCache()
        {
            _cache.ClearCache();
        }
    }
}
