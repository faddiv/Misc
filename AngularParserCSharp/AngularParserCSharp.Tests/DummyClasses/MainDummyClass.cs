using System.Collections.Generic;

namespace AngularParserCSharp.Tests.DummyClasses
{
    public class MainDummyClass
    {
        public DummyClass SubDummy { get; set; }

        public List<DummyClass> SubDummies { get; set; }

        public MainDummyClass()
        {
        }
    }
}