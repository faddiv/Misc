using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Blazorify.Utilities.Styling
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCssBuilder(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ICssBuilderCache, ThreadsafeCssBuilderCache>();
            serviceCollection.TryAddSingleton<ICssBuilderNamingConvention, DefaultCssBuilderNamingConvention>();
            serviceCollection.TryAddTransient<ICssBuilder, CssBuilder>();
            serviceCollection.TryAddTransient(CssBuilderDelegateFactory);
        }

        private static CssBuilderDelegate CssBuilderDelegateFactory(IServiceProvider arg)
        {
            return (object[] values) =>
            {
                var cssBuilder = arg.GetService<ICssBuilder>();
                return cssBuilder.AddMultiple(values);
            };
        }
    }
}
