using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
            builderDescription.ImplementationType.Should().Be<CssDefinition>();
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void AddCssBuilder_registers_CssBuilderOptions()
        {
            ServiceCollection coll = new ServiceCollection();
            coll.AddCssBuilder();

            var builderDescription = coll.Should().Contain(sd => sd.ServiceType == typeof(CssBuilderOptions)).Which;
            builderDescription.Lifetime.Should().Be(ServiceLifetime.Singleton);
            builderDescription.ImplementationFactory.Should().NotBeNull();
        }

        [Fact]
        public void AddCssBuilder_uses_options_Action()
        {
            ServiceCollection coll = new ServiceCollection();
            var called = false;
            coll.AddCssBuilder((o) => called = true);

            var serviceProvider = coll.BuildServiceProvider();
            var builder = serviceProvider.GetService<ICssBuilder>();
            builder.Should().NotBeNull();
            ((CssDefinition)builder).Options.Should().NotBeNull();
            called.Should().BeTrue();
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

    }
}
