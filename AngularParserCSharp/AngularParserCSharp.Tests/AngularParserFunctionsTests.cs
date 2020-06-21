using System;
using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    public class AngularParserFunctionsTests
    {
        private AngularCompiler _compiler;
        [SetUp]
        public void Setup()
        {
            _compiler = new AngularCompiler();
        }

        [Test]
        public void Parses_a_function_call()
        {
            //Assert
            var value = new Func<int>(() => 42);
            var scope = new JavascriptObject
            {
                {
                    "aFunction",
                    value
                }
            };
            var method = _compiler.Parse<int>("aFunction()");
            method(scope, null).Should().Be(42);
        }

        [Test]
        public void Parses_a_function_call_with_a_single_number_argument()
        {
            //Assert
            var value = new Func<double, double>(i => i);
            var scope = new JavascriptObject
            {
                {
                    "aFunction",
                    value
                }
            };
            var method = _compiler.Parse<double>("aFunction(42)");
            method(scope, null).Should().Be(42);

        }

        [Test]
        public void Parses_a_function_call_with_a_single_identifier_argument()
        {
            //Assert
            var value = new Func<double, double>(i => i);
            var scope = new JavascriptObject
            {
                {
                    "aFunction",
                    value
                },
                {
                    "n",
                    42.0
                }
            };
            var method = _compiler.Parse<double>("aFunction(n)");
            method(scope, null).Should().Be(42);
        }

        [Test]
        public void Parses_a_function_call_with_a_single_function_call_argument()
        {
            //Assert
            var value = new Func<double, double>(i => i);
            var value2 = new Func<double>(() => 42);
            var scope = new JavascriptObject
            {
                {
                    "aFunction",
                    value
                },
                {
                    "anotherFunction",
                    value2
                }
            };
            var method = _compiler.Parse<double>("aFunction(anotherFunction())");
            method(scope, null).Should().Be(42);

        }

        [Test]
        public void Parses_a_function_call_with_multiple_arguments()
        {
            var aFunction = new Func<double, double, double, double>((i, j, k) => i + j + k);
            var anotherFunction = new Func<double>(() => 30);
            var scope = new JavascriptObject
            {
                {
                    "aFunction",
                    aFunction
                },
                {
                    "anotherFunction",
                    anotherFunction
                },
                {
                    "n",
                    10
                }
            };
            var method = _compiler.Parse<double>("aFunction(2, n, anotherFunction())");
            method(scope, null).Should().Be(42);
        }
    }
}
