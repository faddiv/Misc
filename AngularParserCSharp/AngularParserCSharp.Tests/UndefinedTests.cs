using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    public class UndefinedTests
    {

        [Test]
        public void Undefined_Cant_Have_Property_throught_add()
        {
            //Assert
            var undefined = JavascriptObject.Undefined;
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                undefined.Add("a", 1);
            });
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                undefined.Add(new KeyValuePair<string, object>("a", 1));
            });
        }

        [Test]
        public void Undefined_Cant_Have_Property_throught_indexer()
        {
            //Assert
            var undefined = JavascriptObject.Undefined;

            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                undefined["a"] = 1;
            });
        }

        [Test]
        public void Unable_to_call_indexer_on_Undefined()
        {
            //Assert
            var undefined = JavascriptObject.Undefined;
            object result = null;
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                result = undefined["a"];
            });
            result.Should().BeNull();
        }

        [Test]
        public void Undefined_Cant_Have_Property_throught_dynamic()
        {
            //Assert
            var undefined = JavascriptObject.Undefined;
            dynamic dyn = undefined;

            Assert.Throws(typeof(RuntimeBinderException), () =>
            {
                dyn.a = 1;
            });
        }

        [Test]
        public void Unable_to_call_dynamic_getter_on_Undefined()
        {
            var undefined = JavascriptObject.Undefined;
            dynamic dyn = undefined;
            object result = null;

            Assert.Throws(typeof(RuntimeBinderException), () =>
            {
                result = dyn.a;
            });
            result.Should().BeNull();
        }
    }
}