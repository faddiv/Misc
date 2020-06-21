using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    public class AngularParserTests
    {
        private AngularCompiler _compiler;

        [SetUp]
        public void Setup()
        {
            _compiler = new AngularCompiler();
        }

        [TestCase("42", 42)]
        [TestCase("0", 0)]
        [TestCase("1", 1)]
        [TestCase("042", 42)]
        [TestCase("4200e-2", 42)]
        [TestCase("42e3", 42000)]
        [TestCase("4200E-2", 42)]
        [TestCase("42E3", 42000)]
        [TestCase("-42", -42)]
        [TestCase("-4200E-2", -42)]
        public void Parse_Can_Parse_Integer(string input, int output)
        {
            var method = _compiler.Parse<int>(input);
            //Assert
            method(null,null).Should().Be(output);
        }

        [TestCase("4.2", 4.2)]
        [TestCase("0.2", 0.2)]
        [TestCase("0.0", 0.0)]
        [TestCase("0.01", 0.01)]
        [TestCase(".42", 0.42)]
        [TestCase(".42e3", 420)]
        [TestCase("4.2e-1", 0.42)]
        [TestCase(".42E3", 420)]
        [TestCase("4.2E-1", 0.42)]
        [TestCase("-4.2", -4.2)]
        [TestCase("-.42", -0.42)]
        [TestCase("-4.2E-1", -0.42)]
        public void Parse_Can_Parse_Float(string input, double output)
        {
            var method = _compiler.Parse<double>(input);
            //Assert
            method(null, null).Should().Be(output);
        }

        [TestCase("4.2e")]
        [TestCase("4.2e-")]
        [TestCase("4.2e-a")]
        public void Parse_Throws_Exception_On_Invalid_Number(string input)
        {
            Assert.That(delegate
            {
                var result = _compiler.Parse<double>(input);
                Console.WriteLine("Should throw error, but instead: {0}", result(null, null));
            }, Throws.TypeOf<AngularException>());
        }

        [TestCase(byte.MaxValue, typeof(byte))]
        [TestCase(short.MaxValue, typeof(short))]
        [TestCase(int.MaxValue, typeof(int))]
        [TestCase(long.MaxValue, typeof(long))]
        [TestCase(byte.MinValue, typeof(byte))]
        [TestCase(short.MinValue, typeof(short))]
        [TestCase(int.MinValue, typeof(int))]
        [TestCase(long.MinValue, typeof(long))]
        [TestCase("1.1", typeof(double))]
        public void Parsing_literal_number_returns_the_the_smallest_possible_type(object testData, Type expectedType)
        {
            var input = testData.ToString();

            var method = _compiler.Parse<object>(input);
            method(null, null).Should().BeOfType(expectedType);
        }

        [TestCase("\"strin1+\"", "strin1+")]
        [TestCase("'strin2('", "strin2(")]
        [TestCase("'42'", "42")]
        [TestCase("\"42\"", "42")]
        [TestCase("\"\"", "")]
        [TestCase("''", "")]
        [TestCase("\" a a \"", " a a ")]
        [TestCase("' s s '", " s s ")]
        [TestCase("'\"'", "\"")]
        [TestCase("\"'\"", "'")]
        public void Parse_Can_Parse_String(string input, string output)
        {
            var method = _compiler.Parse<string>(input);
            //Assert
            method(null, null).Should().Be(output);
        }

        [TestCase("\"\\\"\\'\\n\\f\\r\\t\\v\"", "\"'\n\f\r\t\v")]
        [TestCase("'\\\"\\'\\n\\f\\r\\t\\v'", "\"'\n\f\r\t\v")]
        [TestCase("' \\u00a0'", " \u00a0")]
        [TestCase("' \\u00A0'", " \u00a0")]
        public void Parse_Can_Parse_String_With_Escapes(string input, string output)
        {
            var method = _compiler.Parse<string>(input);
            //Assert
            method(null, null).Should().Be(output);
        }

        [TestCase("\"noclosing")]
        [TestCase("'noclosing")]
        [TestCase("\"mismatch'")]
        [TestCase("'mismatch\"")]
        [TestCase("\"bs\"d\"")]
        [TestCase("'bs'd'")]
        [TestCase("\"\\u010\"")]
        [TestCase("'\\u010'")]
        [TestCase("\"\\u010k\"")]
        [TestCase("'\\u010k'")]
        public void Parse_Throws_Exception_On_Invalid_String(string input)
        {
            Assert.That(() =>
                {
                    _compiler.Parse<string>(input)(null, null);
                },
                Throws.TypeOf<AngularException>());
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("null", null)]
        public void Parse_Can_Parse_Identifier_Tokens(string input, object output)
        {
            var method = _compiler.Parse<object>(input);
            //Assert
            method(null, null).Should().Be(output);
        }

        [TestCase(" \n42", (byte)42)]
        [TestCase("42\t ", (byte)42)]
        [TestCase(" true ", true)]
        [TestCase(" ' ' ", " ")]
        [TestCase("42\r\n", (byte)42)]
        [TestCase("42\v", (byte)42)]
        [TestCase("42\u00A0", (byte)42)]
        public void Parse_Ignores_Whitespace(string input, object output)
        {
            var method = _compiler.Parse<object>(input);
            //Assert
            method(null, null).Should().Be(output);
        }

        [TestCase("[]", new object[0], Description = "will parse empty array")]
        [TestCase("[1]", new object[] { 1 }, Description = "will parse single element array")]
        [TestCase("[1, '2', true, [4, 5]]", new object[] { 1, "2", true, new object[] { 4, 5 } }, Description = "can parse multi element array")]
        [TestCase("[1,2,3, ]", new object[] { 1, 2, 3 }, Description = "will parse array with trailing comma")]
        public void Parse_Will_Parse_Arrays(string input, object[] output)
        {
            var method = _compiler.Parse<object[]>(input);
            //Assert
            var array = method(null, null);
            array.Should().NotBeNull();
            array.Should().HaveCount(output.Length);
            array.ShouldAllBeEquivalentTo(output);
        }

        [Test]
        public void Parse_Will_Parse_Empty_Object()
        {
            Parse_will_parse_objects("{}", new Dictionary<string, object>());
        }


        [Test]
        public void Parse_Will_Parse_A_Non_Empty_Object()
        {
            Parse_will_parse_objects("{a:1,b:'value'}", new Dictionary<string, object>
            {
                {"a", 1 },
                {"b", "value" }
            });
        }

        [Test]
        public void Parse_Will_Parse_string_keyed_Object()
        {
            Parse_will_parse_objects("{\"A-key\":1,'b_key':2}", new Dictionary<string, object>
            {
                {"A-key", 1 },
                {"b_key", 2 }
            });
        }

        public Expression Make(Expression<Func<Dictionary<string, object>>> expression)
        {
            return expression;
        }

        [Test]
        public void Parse_Will_Parse_number_keyed_Object()
        {
            Parse_will_parse_objects("{1:1}", new Dictionary<string, object>
            {
                {"1", 1 }
            });
        }

        [Test]
        public void Parse_Will_Parse_object_with_empty_last_element()
        {
            Parse_will_parse_objects("{a:2,}", new Dictionary<string, object>
            {
                {"a", 2 }
            });
        }

        [Test]
        public void Parse_Will_Parse_composite_value_in_object()
        {
            Parse_will_parse_objects("{a:{a:1, b:2}, b: [1,2]}", new Dictionary<string, object>
            {
                {"a", new Dictionary<string, object>
                {
                    {"a",1 },
                    {"b", 2 }
                } },
                {"b", new object[]{1,2} }
            });
        }

        private void Parse_will_parse_objects(string input, object output)
        {
            var method = _compiler.Parse<object>(input);
            //Assert
            var value = method(null, null);
            value.Should().NotBeNull();
            value.ShouldBeEquivalentTo(output);
        }
    }
}
