using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Blazorify.Utilities.Styles
{
    public class EnumExperimentsTests
    {
        [Fact]
        public void EnumsTest()
        {
            Enum enum1 = Dummy.c10;
            Enum enum2 = Dummy2.c20;

            (enum1 == enum2).Should().BeFalse();
        }

        [Fact]
        public void EnumsTest2()
        {
            Enum enum1 = Dummy.c10;
            Enum enum2 = Dummy2.c20;

            (enum1.Equals(enum2)).Should().BeFalse();
        }

        [Fact]
        public void EnumsTest3()
        {
            var dic = new Dictionary<Enum, string>();
            dic.Add(Dummy.c10, "c-10");
            dic.Add(Dummy2.c20, "c-20");

            dic[Dummy.c10].Should().Be("c-10");
            dic[Dummy2.c20].Should().Be("c-20");
        }

        [Fact]
        public void EnumsTest4()
        {
            Enum enum1 = Dummy.c10;
            Enum enum2 = Dummy2.c20;

            GetHashCode(enum1).Should().NotBe(GetHashCode(enum2));
        }

        public int GetHashCode(Enum value)
        {
            int hashCode = -1959444751;
            hashCode = hashCode * -1521134295 + value.GetType().GetHashCode();
            hashCode = hashCode * -1521134295 + value.GetHashCode();
            return hashCode;
        }

        public enum Dummy
        {
            NameName_name = 1,
            c10 = 2
        }
        public enum Dummy2
        {
            NameName_name = 1,
            c20 = 2
        }
    }
}
