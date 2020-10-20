using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;

namespace Blazorify.Utilities.Styling
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCssBuilder(this IServiceCollection serviceCollection,
            ICssBuilderCache builderCache = null,
            ICssBuilderNamingConvention namingConvention = null)
        {
            builderCache ??= (RuntimeInformation.OSDescription != "web")
                ? new ThreadsafeCssBuilderCache()
                : (ICssBuilderCache)new ThreadUnsafeCssBuilderCache();
            serviceCollection.AddSingleton(builderCache);

            namingConvention ??= new DefaultCssBuilderNamingConvention();
            serviceCollection.AddSingleton(namingConvention);

            serviceCollection.AddTransient<ICssBuilder, CssBuilder>();

            serviceCollection.AddTransient(CssBuilderDelegateFactory);
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
