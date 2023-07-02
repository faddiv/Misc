using System;

namespace Mediatr.Analyzers.Test.Utilities
{
    static class MediatRCode
    {
        public const string Source = @"
namespace MediatR
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRequest : IBaseRequest { }

    public interface IRequest<out TResponse> : IBaseRequest { }

    public interface IBaseRequest { }

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IRequestHandler<in TRequest>
        where TRequest : IRequest
    {
        Task Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IStreamRequest<out TResponse> { }

    public interface IStreamRequestHandler<in TRequest, out TResponse>
        where TRequest : IStreamRequest<TResponse>
    {
        IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface INotification { }

    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
";
        public const string defaultNamespaces = @"using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;";
        public static string AppendMediatRCode(string code)
        {
            return code + Source;
        }
        public static string WithNamespaces(this string code)
        {
            return defaultNamespaces + code;
        }
    }
}
