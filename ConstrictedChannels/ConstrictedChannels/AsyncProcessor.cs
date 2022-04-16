using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ConstrictedChannels
{
    public class AsyncProcessor
    {
        private readonly ConcurrentDictionary<string, (TaskCompletionSource<Response> TaskSource, CancellationTokenSource TokenSource)> _requests;
        private readonly BlockingCollection<RequestProcessorSide> _channels;

        public TimeSpan Timeout { get; }
        public int FreeChannel => _channels.Count;

        public AsyncProcessor(int channelCount = 1, TimeSpan? timeout = null)
        {
            _requests = new ConcurrentDictionary<string, (TaskCompletionSource<Response>, CancellationTokenSource)>();
            _channels = new BlockingCollection<RequestProcessorSide>(channelCount);
            Timeout = timeout ?? TimeSpan.FromSeconds(30);
            for (int i = 0; i < channelCount; i++)
            {
                var proc = new RequestProcessorSide();
                proc.SendResponse += ProcessResponse;
                _channels.Add(proc);
            }
        }

        public async Task<Response> RemoteCall(Request request, CancellationToken token = default)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            var taskCompletionSource = new TaskCompletionSource<Response>(cancellationTokenSource);
            if (!_requests.TryAdd(request.RequestId, (taskCompletionSource, cancellationTokenSource)))
            {
                throw new Exception("Add failed for some reason");
            }
            cancellationTokenSource.Token.Register(() =>
            {
                if (taskCompletionSource.Task.IsCompleted)
                    return;
                _requests.TryRemove(request.RequestId, out _);
                taskCompletionSource.TrySetCanceled();
                cancellationTokenSource.Dispose();
            });
            cancellationTokenSource.CancelAfter(Timeout);
            if (_channels.TryTake(out var proc, -1, token))
            {
                try
                {
                    await proc.SendRequest(request, token);
                }
                finally
                {
                    _channels.Add(proc);
                }
            }
            else
            {
                token.ThrowIfCancellationRequested();
                throw new Exception("Take Channel failed");
            }
            return await taskCompletionSource.Task;
        }

        private void ProcessResponse(Response response)
        {
            if(_requests.TryRemove(response.RequestId, out var taskInfo))
            {
                taskInfo.TaskSource.TrySetResult(response);
                taskInfo.TokenSource.Dispose();
            }
        }

    }
}
