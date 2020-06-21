using AngularParserCSharp.Tests.DummyClasses;
using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    public class AssigningValuesTests
    {

        [SetUp]
        public void Setup()
        {
            _compiler = new AngularCompiler();
        }

        private AngularCompiler _compiler;

        [Test]
        public void Will_parses_a_simple_attribute_assignment()
        {
            var o = new DummyClass(0);
            var method = _compiler.Parse<DummyClass, DummyClass, object>("Key = 42");
            method(o, null).Should().Be(42);
            o.Key.Should().Be(42);
        }

        [Test]
        public void Can_assign_any_primary_expression()
        {
            var o = new DummyClass(0);
            var method = _compiler.Parse<DummyClass, DummyClass, object>("Key = ReturnEverything()");
            method(o, null).Should().Be(42);
            o.Key.Should().Be(42);
        }

        [Test]
        public void Can_assign_a_computed_object_property()
        {
            var o = new JavascriptObject
            {
                {
                    "key", "property"
                },
                {
                    "property", 0
                }
            };
            var method = _compiler.Parse<object>("this[this.key] = 42");
            method(o, null).Should().Be(42);
            o["property"].Should().Be(42);
        }

        [Test]
        public void Can_assign_on_property_chain()
        {
            var o = new MainDummyClass
            {
                SubDummy = new DummyClass(0)
            };
            var method = _compiler.Parse<MainDummyClass, MainDummyClass, int>("SubDummy.Key = 42");
            method(o, null).Should().Be(42);
            o.SubDummy.Key.Should().Be(42);
        }

        [Test]
        public void Can_assign_on_indexer()
        {
            var o = new MainDummyClass
            {
                SubDummies = new System.Collections.Generic.List<DummyClass>
                {
                    new DummyClass(0)
                }
            };
            var method = _compiler.Parse<MainDummyClass, MainDummyClass, object>("SubDummies[0].Key = 42");
            method(o, null).Should().Be(42);
            o.SubDummies[0].Key.Should().Be(42);
        }
    }
}