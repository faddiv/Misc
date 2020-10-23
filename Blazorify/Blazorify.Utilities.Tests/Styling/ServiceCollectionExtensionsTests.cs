using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Xunit;

namespace Blazorify.Utilities.Styling
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCssBuilder_registers_CssBuilder()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddCssBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(ICssBuilder)).Which;
            builderDescription.ImplementationType.Should().Be<CssBuilder>();
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void AddCssBuilder_registers_DefaultCssBuilderNamingConvention()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddCssBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(ICssBuilderNamingConvention)).Which;
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Singleton);
            builderDescription.ImplementationType.Should().Be<DefaultCssBuilderNamingConvention>();
        }

        [Fact]
        public void AddCssBuilder_registers_caching()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddCssBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(ICssBuilderCache)).Which;
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Singleton);
            builderDescription.ImplementationType.Should().Be<ThreadsafeCssBuilderCache>();
        }

        [Fact]
        public void AddCssBuilder_registers_custom_NamingConvention()
        {
            ServiceCollection coll = new ServiceCollection();
            OtherNamingConvention namingConvention = new OtherNamingConvention();

            coll.AddSingleton<ICssBuilderNamingConvention>(namingConvention);
            coll.AddCssBuilder();

            coll.Should().ContainSingle(sd => sd.ServiceType == typeof(ICssBuilderNamingConvention));
        }

        [Fact]
        public void AddCssBuilder_registers_custom_Cache()
        {
            ServiceCollection coll = new ServiceCollection();
            OtherCache cache = new OtherCache();

            coll.AddSingleton<ICssBuilderCache>(cache);
            coll.AddCssBuilder();

            coll.Should().ContainSingle(sd => sd.ServiceType == typeof(ICssBuilderCache));
        }

        [Fact]
        public void AddCssBuilder_registers_CssBuilderDelegate()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddCssBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(CssBuilderDelegate)).Which;
            builderDescription.ImplementationFactory.Should().NotBeNull();
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void CssBuilderDelegate_processes_the_input_params()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddCssBuilder();
            var provider = coll.BuildServiceProvider();
            var css = provider.GetService<CssBuilderDelegate>();

            var result = css("c1", ("c2", true)).ToString();

            result.Should().Be("c1 c2");
        }

        [Fact]
        public void AddStyleBuilder_registers_StyleBuilder()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddStyleBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(IStyleBuilder)).Which;
            builderDescription.ImplementationType.Should().Be<StyleBuilder>();
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void AddStyleBuilder_registers_StyleBuilderDelegate()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddStyleBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(StyleBuilderDelegate)).Which;
            builderDescription.ImplementationFactory.Should().NotBeNull();
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        private class OtherCache : ICssBuilderCache
        {
            public ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create)
            {
                throw new NotImplementedException();
            }

            public string GetOrAdd(Enum value, Func<Enum, string> create)
            {
                throw new NotImplementedException();
            }
        }

        private class OtherNamingConvention : ICssBuilderNamingConvention
        {
            public string ToCssClassName(PropertyInfo property)
            {
                throw new NotImplementedException();
            }

            public string ToCssClassName(Enum enumValue)
            {
                throw new NotImplementedException();
            }
        }
    }
}
