namespace AngularParserCSharp.Tests.DummyClasses
{
    public class DummyClass
    {
        #region  Constructors

        public DummyClass(int key)
        {
            Key = key;
        }

        #endregion

        #region Properties

        public JavascriptObject DynamicProp { get; set; }
        public int Key { get; set; }

        #endregion

        #region  Public Methods

        public int ReturnEverything()
        {
            return 42;
        }

        public void SetKey(int key)
        {
            Key = key;
        }

        #endregion
    }
}