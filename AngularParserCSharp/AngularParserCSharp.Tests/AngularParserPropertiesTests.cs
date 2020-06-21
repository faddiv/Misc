using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    class AngularParserPropertiesTests
    {
        [SetUp]
        public void Setup()
        {
            _compiler = new AngularCompiler();
        }

        private AngularCompiler _compiler;

        [Test]
        public void Looks_up_an_property_from_the_scope()
        {
            //Assert
            var method = _compiler.Parse<int>("aKey");
            //Assert
            var obj = new JavascriptObject();
            dynamic dyn = obj;
            dyn.aKey = 42;
            method(obj, null).Should().Be(42);
        }

        [Test]
        public void Returns_undefined_when_looking_up_property_from_undefined()
        {
            //Assert
            var method = _compiler.Parse<JavascriptObject>("aKey");
            //Assert
            method(JavascriptObject.Undefined, null).Should().BeSameAs(JavascriptObject.Undefined);
        }

        [Test]
        public void Will_parse_this()
        {
            //Assert
            var scope = new JavascriptObject();
            var method = _compiler.Parse<JavascriptObject>("this");
            method(scope, null).Should().BeSameAs(scope);
        }

        [Test]
        public void Handles_undefined_on_2_part_identifier_path()
        {
            //Assert
            var obj = new JavascriptObject();
            var method = _compiler.Parse<object>("key.key2");
            method(obj, null).Should().BeSameAs(JavascriptObject.Undefined);
        }

        [Test]
        public void Looks_up_a_2_part_identifier_path_from_the_scope()
        {
            //Assert
            var obj = new JavascriptObject
            {
                {
                    "key", new JavascriptObject
                    {
                        {"key2", 42}
                    }
                }
            };
            var method = _compiler.Parse<object>("key.key2");
            method(obj, null).Should().Be(42);
        }

        [Test]
        public void Looks_up_a_member_from_an_object()
        {
            //Assert
            var method = _compiler.Parse<object>("{ key: 42}.key");
            method(null, null).Should().Be((byte)42);
        }

        [Test]
        public void Looks_up_a_n_part_identifier_path_from_the_scope()
        {
            //Assert
            var obj = new JavascriptObject
            {
                {
                    "key", new JavascriptObject
                    {
                        {
                            "key2", new JavascriptObject
                            {
                                {
                                    "key3", new JavascriptObject
                                    {
                                        {"key4", 42}
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var method = _compiler.Parse<object>("key.key2.key3.key4");
            method(obj, null).Should().Be(42);
        }

    }
    //Method Calls
    // page: 251
}