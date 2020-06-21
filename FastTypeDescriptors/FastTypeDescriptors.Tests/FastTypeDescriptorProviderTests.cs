using System;
using System.ComponentModel;
using FastTypeDescriptors.SpeedTests;
using FluentAssertions;
using Xunit;

namespace FastTypeDescriptors
{
    public class FastTypeDescriptorProviderTests : IDisposable
    {
        public void Dispose()
        {
            TypeDescriptor.RemoveProvider(FastTypeDescriptorProvider.Instance, typeof(object));
        }

        [Fact]
        public void RegisterRegistersTheFastTypeDescriptorProvider()
        {
            FastTypeDescriptorProvider.Register(typeof(object));
            var typeDescriptionProvider = TypeDescriptor.GetProvider(typeof(Poco));
            var typeDescriptor = TypeDescriptor.GetProperties(typeof(Poco));
            //typeDescriptor.Should().BeOfType(typeof(FastTypeDescriptor));
            
        }

        [Fact]
        public void ReturnsFastPropertyDescriptor()
        {
            FastTypeDescriptorProvider.Register(typeof(object));
            var typeDescriptionProvider = TypeDescriptor.GetProvider(typeof(Poco));
            var props = TypeDescriptor.GetProperties(typeof(Poco));
            var inst = new Poco();
            var id = Guid.NewGuid();
            props["Id"].SetValue(inst, id);
            props["Id"].GetValue(inst).Should().Be(id);

            props["Value"].SetValue(inst, id.ToString());
            props["Value"].GetValue(inst).Should().Be(id.ToString());
        }
    }
}
