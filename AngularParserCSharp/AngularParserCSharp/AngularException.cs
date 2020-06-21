using System;
using System.Runtime.Serialization;

namespace AngularParserCSharp
{
    [Serializable]
    public class AngularException : Exception
    {
        #region  Constructors

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AngularException()
        {
        }

        public AngularException(string message) : base(message)
        {
        }

        public AngularException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AngularException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }
}