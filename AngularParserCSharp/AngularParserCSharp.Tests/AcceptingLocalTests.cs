using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    class AcceptingLocalTests
    {
        private AngularCompiler _compiler;

        [SetUp]
        public void Setup()
        {
            _compiler = new AngularCompiler();
        }

        [Test]
        public void Uses_locals_instead_of_scope_when_there_is_matching_key()
        {
            //Assert
            var method = _compiler.Parse<int>("aKey");
            var scope = new JavascriptObject
            {
                {"aKey", 43}
            };
            var locals = new JavascriptObject
            {
                {"aKey", 42}
            };
            method(scope, locals).Should().Be(42);
        }

        [Test]
        public void Does_not_use_locals_instead_of_scope_when_no_matching_key()
        {
            //Assert
            var method = _compiler.Parse<int>("aKey");
            var scope = new JavascriptObject
            {
                {"aKey", 43}
            };
            var locals = new JavascriptObject
            {
                {"anotherKey", 42}
            };
            method(scope, locals).Should().Be(43);
        }

        [Test]
        public void Uses_locals_instead_of_scope_when_the_first_part_matches()
        {
            //Assert
            var method = _compiler.Parse<object>("aKey.anotherKey");
            var scope = new JavascriptObject
            {
                {"aKey", new JavascriptObject
                {
                    {"anotherKey", 42 }
                }}
            };
            var locals = new JavascriptObject
            {
                {"aKey", null}
            };
            method(scope, locals).Should().Be(JavascriptObject.Undefined);
        }
    }
}
