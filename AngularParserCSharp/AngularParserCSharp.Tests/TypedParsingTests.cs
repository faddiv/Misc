using AngularParserCSharp.Tests.DummyClasses;
using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    public class TypedParsingTests
    {
        [SetUp]
        public void Setup()
        {
            _compiler = new AngularCompiler();
        }

        private AngularCompiler _compiler;

        [Test]
        public void Will_parse_on_plain_object_scope()
        {
            var o = new DummyClass(42);
            var method = _compiler.Parse<DummyClass, DummyClass, object>("Key");
            method(o, null).Should().Be(42);
        }

        [Test]
        public void Will_parse_property_chain_on_plain_object_scope()
        {
            var o = new MainDummyClass
            {
                SubDummy = new DummyClass(42)
            };
            var method = _compiler.Parse<MainDummyClass, MainDummyClass, object>("SubDummy.Key");
            method(o, null).Should().Be(42);
        }

        [Test]
        public void Will_parse_property_getter_on_this()
        {
            var o = new DummyClass(42);
            var method = _compiler.Parse<DummyClass, DummyClass, object>("this.Key");
            method(o, null).Should().Be(42);
        }

        [Test]
        public void Will_parse_property_chain_on_this()
        {
            var o = new MainDummyClass
            {
                SubDummy = new DummyClass(42)
            };
            var method = _compiler.Parse<MainDummyClass, MainDummyClass, object>("this.SubDummy.Key");
            method(o, null).Should().Be(42);
        }

        [Test]
        public void Will_parse_method_call_on_this()
        {
            var o = new DummyClass(0);
            var method = _compiler.Parse<DummyClass, DummyClass, object>("this.SetKey(42)");
            method(o, null).Should().Be(42);
            o.Key.Should().Be(42);
    }

        [Test]
        public void Will_parse_method_call_on_context()
        {
            var o = new DummyClass(0);
            var method = _compiler.Parse<DummyClass, DummyClass, object>("SetKey(42)");
            method(o, null).Should().Be(42);
            o.Key.Should().Be(42);
        }


        [Test]
        public void Can_use_concrete_type_on_JavascriptObject()
        {
            var o = new DummyClass(0);
            var r = new JavascriptObject
            {
                {"AProperty", o }
            };
            var method = _compiler.Parse<JavascriptObject, DummyClass, object>("AProperty.SetKey(42)");
            method(r, null).Should().Be(42);
            o.Key.Should().Be(42);
        }

        [Test]
        public void Can_use_JavascriptObject_on_concrete_type()
        {
            var r = new JavascriptObject
            {
                {"AProperty", 42 }
            };
            var o = new DummyClass(0)
            {
                DynamicProp = r
            };
            var method = _compiler.Parse<DummyClass, DummyClass, object>("DynamicProp.AProperty");
            method(o, null).Should().Be(42);
        }
    }
}