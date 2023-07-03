using System.Diagnostics;
using System.Threading;

namespace MediatR.Analyzers.Utilities
{
    public class Logger
    {
        [Conditional("DEBUG")]
        public static void Log(string text)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine($"thr({threadId}) INF {text}");
        }

        [Conditional("DEBUG")]
        public static void Log(string text, params object[] param1)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine($"thr({threadId}) INF {text}", param1);
        }

    }
}
