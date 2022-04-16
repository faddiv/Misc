using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConstrictedChannels
{
    public class RequestProcessorSide
    {
        private readonly Random _rnd = new Random();
        public event Action<Response> SendResponse;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task SendRequest(Request request, CancellationToken token)
        {
            var type = _rnd.Next(5);
            var t1 = type == 1 || type == 3 ? "SlowRequest" : "";
            var t2 = type == 2 || type == 3 ? "SlowResponse" : "";
            request.Type = 0  < type && type < 4 ? $"{t1} {t2}" : "FastRequest";
            await _semaphore.WaitAsync(token);
            try
            {
                var secs = type == 1 || type == 3
                    ? _rnd.Next(3000, 10001)
                    : _rnd.Next(100, 500);
                await Task.Delay(secs, token);
                var thread = new Thread((state) =>
                {
                    var response = new Response(state as Request);
                    //Console.WriteLine($"{response} Generating Response Started. {response.RequestTime}");
                    var secs2 = type == 2 || type == 3
                    ? _rnd.Next(1000, 5001)
                    : _rnd.Next(100, 500);
                    Thread.Sleep(secs2);
                    //Console.WriteLine($"{response} Generating Response Finished");
                    OnSendResponse(response);
                });
                thread.Start(request);
            }
            finally
            {
                _semaphore.Release();
            }
            //Console.WriteLine($"{request} SendRequest Finished");
        }

        private void OnSendResponse(Response response)
        {
            response.Stopwatch.Stop();
            SendResponse?.Invoke(response);
        }
    }
}
