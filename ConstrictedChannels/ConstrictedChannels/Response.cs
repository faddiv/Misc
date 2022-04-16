using System;
using System.Diagnostics;

namespace ConstrictedChannels
{
    public class Response
    {
        public Response(Request request)
        {
            RequestId = request.RequestId;
            RequestTime = request.Stopwatch.Elapsed;
            Stopwatch = request.Stopwatch;
        }
        public string RequestId { get; set; }
        public TimeSpan RequestTime { get; set; }
        public TimeSpan ResponseTime => Stopwatch?.Elapsed ?? TimeSpan.Zero;
        public Stopwatch Stopwatch { get; set; }

        public override string ToString()
        {
            return $"{RequestId} Response:";
        }
    }
}
