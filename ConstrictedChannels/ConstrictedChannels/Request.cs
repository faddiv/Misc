using System;
using System.Diagnostics;

namespace ConstrictedChannels
{
    public class Request
    {
        public Request()
        {
            RequestId = Guid.NewGuid().ToString();
            Stopwatch = Stopwatch.StartNew();
        }

        public string RequestId { get; set; }

        public Stopwatch Stopwatch { get; set; }
        public string Type { get; internal set; } = "Unset";

        public override string ToString()
        {
            return $"{RequestId} Request:";
        }
    }
}
