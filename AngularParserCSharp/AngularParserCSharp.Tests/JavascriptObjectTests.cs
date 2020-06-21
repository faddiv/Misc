using System.Collections.Generic;
using System.Dynamic;
using FluentAssertions;
using NUnit.Framework;

namespace AngularParserCSharp.Tests
{
    [TestFixture]
    public class JavascriptObjectTests
    {
        [Test]
        public void Can_created_with_dynamic_features()
        {
            //Assert
            var javascriptObject = new JavascriptObject();

            javascriptObject.Should().BeAssignableTo(typeof(DynamicObject));
            javascriptObject.Should().BeAssignableTo(typeof(IDictionary<string, object>));
        }

        [Test]
        public void Should_act_like_dictionary()
        {
            var javascriptObject = new JavascriptObject
            {
                {"a", 1},
                {"b", "c"}
            };
            javascriptObject.ShouldBeEquivalentTo(new Dictionary<string, object>()
            {
                {"a", 1},
                {"b", "c"}
            });
        }

        [Test]
        public void Properties_should_be_settable_through_dynamic()
        {
            //Assert
            var javascriptObject = new JavascriptObject();
            dynamic dyn = javascriptObject;
            dyn.a = 1;
            dyn.b = "c";
            javascriptObject.ShouldBeEquivalentTo(new Dictionary<string, object>()
            {
                {"a", 1},
                {"b", "c"}
            });
        }

        [Test]
        public void Properties_should_be_gettable_through_dynamic()
        {
            //Assert
            var javascriptObject = new JavascriptObject
            {
                {"a", 1},
                {"b", "c"}
            };
            dynamic dyn = javascriptObject;
            object a = dyn.a;
            object b = dyn.b;
            a.Should().Be(1);
            b.Should().Be("c");
        }

        [Test]
        public void JavascriptObject_has_An_Undefined_Instance()
        {
            //Assert
            var undefined = JavascriptObject.Undefined;
            undefined.Should().NotBeNull();
        }

        [Test]
        public void Returns_undefined_on_non_defined_indexer()
        {
            //Assert
            // ReSharper disable once CollectionNeverUpdated.Local
            var obj = new JavascriptObject();
            obj["nothing"].Should().BeSameAs(JavascriptObject.Undefined);
        }

        [Test]
        public void Returns_undefined_on_non_defined_dynamic()
        {
            //Assert
            var obj = new JavascriptObject();
            dynamic dyn = obj;
            object nothing = dyn.nothing;
            nothing.Should().BeSameAs(JavascriptObject.Undefined);
        }
    }
}