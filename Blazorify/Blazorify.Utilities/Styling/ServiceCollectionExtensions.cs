using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Blazorify.Utilities.Styling
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCssBuilder(this IServiceCollection serviceCollection,
            Action<CssBuilderOptions> action = null)
        {
            serviceCollection.TryAddTransient<ICssBuilder, CssBuilder>();
            serviceCollection.AddSingleton(p =>
            {
                var options = new CssBuilderOptions();
                action?.Invoke(options);
                return options;
            });
        }

        public static void AddStyleBuilder(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddTransient<IStyleBuilder, StyleBuilder>();
            serviceCollection.TryAddTransient(StyleBuilderDelegateFactory);
        }

        private static StyleBuilderDelegate StyleBuilderDelegateFactory(IServiceProvider arg)
        {
            return (object[] values) =>
            {
                var styleBuilder = arg.GetService<IStyleBuilder>();
                return styleBuilder.AddMultiple(values);
            };
        }
    }
}
