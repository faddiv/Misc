using Blazorify.Utilities.Styling;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Blazorify.Utilities.Styles
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
            builderDescription.ImplementationInstance.Should().BeOfType<DefaultCssBuilderNamingConvention>();
        }

        [Fact]
        public void AddCssBuilder_registers_one_Caching()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddCssBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(ICssBuilderCache)).Which;
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Singleton);
            builderDescription.ImplementationInstance.Should().NotBeNull();
        }

        [Fact]
        public void AddCssBuilder_registers_custom_NamingConvention()
        {
            ServiceCollection coll = new ServiceCollection();
            OtherNamingConvention namingConvention = new OtherNamingConvention();
            coll.AddCssBuilder(namingConvention: namingConvention);

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(ICssBuilderNamingConvention)).Which;
            builderDescription.ImplementationInstance.Should().BeSameAs(namingConvention);
        }

        [Fact]
        public void AddCssBuilder_registers_custom_Cache()
        {
            ServiceCollection coll = new ServiceCollection();
            OtherCache cache = new OtherCache();
            coll.AddCssBuilder(builderCache: cache);

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(ICssBuilderCache)).Which;
            builderDescription.ImplementationInstance.Should().BeSameAs(cache);
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

        public class OtherCache : ICssBuilderCache
        {
            public ProcessObjectDelegate GetOrAdd(Type type, Func<Type, ProcessObjectDelegate> create)
            {
                throw new NotImplementedException();
            }
        }

        public class OtherNamingConvention : ICssBuilderNamingConvention
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
