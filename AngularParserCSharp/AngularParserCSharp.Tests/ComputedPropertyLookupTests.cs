using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    class ComputedPropertyLookupTests
    {
        [SetUp]
        public void Setup()
        {
            _compiler = new AngularCompiler();
        }

        private AngularCompiler _compiler;

        [Test]
        public void Parses_a_simple_computed_property_access()
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
            var method = _compiler.Parse<int>("key['key2']");
            method(obj, null).Should().Be(42);
        }

        [Test]
        public void Parses_a_computed_numeric_array_access()
        {
            //Assert
            var method = _compiler.Parse<object>("aKey[1]");
            var scope = new JavascriptObject
            {
                {"aKey", new [] {1,2,3}}
            };
            method(scope, null).Should().Be(2);
        }

        [Test]
        public void Parses_a_computed_access_with_another_key_as_property()
        {
            //Assert
            var obj = new JavascriptObject
            {
                {
                    "lock", new JavascriptObject
                    {
                        {"key2", 42}
                    }
                },
                {
                    "key", "key2"
                }
            };
            var method = _compiler.Parse<int>("lock[key]");
            method(obj, null).Should().Be(42);
        }
        [Test]
        public void Parses_computed_access_with_another_access_as_property()
        {
            //Assert
            var obj = new JavascriptObject
            {
                {
                    "lock", new JavascriptObject
                    {
                        {"key2", 42}
                    }
                },
                {
                    "keys", new JavascriptObject
                    {
                        {
                            "theKey", "key2"
                        }
                    }
                }
            };
            var method = _compiler.Parse<int>("lock[keys['theKey']]");
            method(obj, null).Should().Be(42);
        }

        [Test]
        public void Parses_computed_property_access_on_this()
        {
            //Assert
            var obj = new JavascriptObject
            {
                {
                    "key", 42
                }
            };
            var method = _compiler.Parse<int>("this['key']");
            method(obj, null).Should().Be(42);
        }

    }
}
