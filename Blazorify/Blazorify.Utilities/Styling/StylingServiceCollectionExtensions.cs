using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Blazorify.Utilities.Styling
{
    public static class StylingServiceCollectionExtensions
    {
        public static void AddCssBuilder(this IServiceCollection serviceCollection,
            Action<CssBuilderOptions> action = null)
        {
            serviceCollection.TryAddSingleton<ICssBuilder, CssBuilder>();
            serviceCollection.AddSingleton(p =>
            {
                var options = new CssBuilderOptions();
                action?.Invoke(options);
                return options;
            });
        }

        public static void AddStyleBuilder(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IStyleBuilder, StyleBuilder>();
        }
    }
}
