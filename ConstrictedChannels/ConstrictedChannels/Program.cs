using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConstrictedChannels
{
    internal class Program
    {
        static ConcurrentBag<Response> _responses = new ConcurrentBag<Response>();
        static int _cancelCount = 0;
        static Random _rnd = new Random();
        static void Main(string[] args)
        {
            var asyncProcessor = new AsyncProcessor(5, TimeSpan.FromSeconds(5));
            var totalTime = Stopwatch.StartNew();
            var threads = new List<Task>();

            for (int i = 0; i < 40; i++)
            {
                var task = Task.Run(async () =>
                {
                    var request = new Request();
                    try
                    {
                        lock(_rnd)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{request} Program Sending a Request. FreeChannel: {asyncProcessor.FreeChannel}");
                        }
                        var response = await asyncProcessor.RemoteCall(request);
                        lock (_rnd)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{response} Program Receiving a Response. FreeChannel: {asyncProcessor.FreeChannel} Type {request.Type}");
                        }
                        _responses.Add(response);
                    }
                    catch (OperationCanceledException)
                    {
                        lock (_rnd)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{request} RemoteCall Cancelled. FreeChannel: {asyncProcessor.FreeChannel} Type {request.Type}");
                        }
                        Interlocked.Increment(ref _cancelCount);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{request} Failed. {ex.GetType().Name}: {ex.Message}\r\n{ex.StackTrace}");
                    }

                });
                Thread.Sleep(_rnd.Next(100, 2000));
                threads.Add(task);
            }
            lock(_rnd)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Wait all task to finish.");
            }
            Task.WaitAll(threads.ToArray());
            totalTime.Stop();
            lock(_rnd)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"All task completed. TotalTime: {totalTime.Elapsed}");
                var avg = TimeSpan.FromMilliseconds(_responses.Count > 0
                    ? _responses.Average(e => e.ResponseTime.TotalMilliseconds)
                    : 0);
                Console.WriteLine($"Average: {avg} Completed/Cancelled: {_responses.Count}/{_cancelCount}");
            }
        }
    }
}
