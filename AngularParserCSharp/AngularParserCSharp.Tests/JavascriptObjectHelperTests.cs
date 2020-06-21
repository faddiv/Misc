using System;
using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    public class JavascriptObjectHelperTests
    {
        private JavascriptObjectHelper _helper;
        [SetUp]
        public void Setup()
        {
            _helper = new JavascriptObjectHelper();
        }
        [TestCase(null, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(42, true)]
        [TestCase(-1, true)]
        [TestCase(0, false)]
        [TestCase(42.1, true)]
        [TestCase(-1.1, true)]
        [TestCase(0.0, false)]
        [TestCase(Double.NaN, false)]
        [TestCase("", false)]
        [TestCase("a", true)]
        public void IsTruthy_should_return_value_based_on_javasript(object value, bool truthy)
        {
            _helper.IsTruthy(value).Should().Be(truthy);
        }
        
        [Test]
        public void IsTruthy_should_return_false_on_undefined()
        {
            IsTruthy_should_return_value_based_on_javasript(JavascriptObject.Undefined, false);
        }

        [Test]
        public void IsTruthy_should_return_true_on_object()
        {
            IsTruthy_should_return_value_based_on_javasript(new JavascriptObject(), true);
        }

        [Test]
        public void IsTruthy_should_return_true_on_array()
        {
            IsTruthy_should_return_value_based_on_javasript(new JavascriptObject[0], true);
        }
    }
}
