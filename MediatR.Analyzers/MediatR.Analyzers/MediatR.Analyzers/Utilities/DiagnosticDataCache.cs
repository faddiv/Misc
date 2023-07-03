using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static MediatR.Analyzers.Utilities.Constants;

namespace MediatR.Analyzers.Utilities
{
    public class DiagnosticDataCache
    {

        private static DiagnosticDataCache _cache;
        private static bool _cacheInitialized = false;
        private static object _cacheLock = new object();

        public static DiagnosticDataCache GetInstance(Compilation compilation)
        {
            return LazyInitializer.EnsureInitialized(ref _cache, ref _cacheInitialized, ref _cacheLock, () =>
            {
                Logger.Log("EnsureCacheInicialized started");
                var requestHandler1 = TypeChecks.GetRequestHandler1(compilation);
                if (requestHandler1 is null)
                    return new DiagnosticDataCache();
                var requestHandler2 = TypeChecks.GetRequestHandler2(compilation);
                if (requestHandler2 is null)
                    return new DiagnosticDataCache();
                var h = new HandlerCollectorSymbolVisitor(requestHandler1, requestHandler2);
                compilation.Assembly.Accept(h);
                Logger.Log("EnsureCacheInicialized finished");
                foreach (var item in h.CollectedHandlers)
                {
                    Logger.Log("  Captured handler {0} reuqest {1}", item.Handler, item.Request);
                }
                return new DiagnosticDataCache(h.CollectedHandlers);
            });
        }

        public ConcurrentDictionary<string, HandlerInfo> Handlers { get; set; }

        public DiagnosticDataCache()
        {
            Handlers = new ConcurrentDictionary<string, HandlerInfo>();
        }

        public DiagnosticDataCache(List<HandlerInfo> handlers)
        {
            Handlers = new ConcurrentDictionary<string, HandlerInfo>(
                handlers.Select(e => new KeyValuePair<string, HandlerInfo>(e.Request, e)));
        }

        internal bool HasHandler(INamedTypeSymbol request, Compilation compilation)
        {
            var requestName = request.ToDisplayString();
            if (!Handlers.TryGetValue(requestName, out var foundHandlerInfo))
            {
                return false;
            }
            var handler = compilation.GetTypeByMetadataName(foundHandlerInfo.Handler);
            if (handler is null)
            {
                Logger.Log("Handler dissapeared {0} -> {1}", foundHandlerInfo.Handler, foundHandlerInfo.Request);
                Handlers.TryRemove(requestName, out _);
                return false;
            }
            return true;
        }

        internal void TryAddNewHandler(INamedTypeSymbol symbol, Compilation compilation)
        {
            var requestHandler1 = TypeChecks.GetRequestHandler1(compilation);
            if (requestHandler1 is null)
                return;
            var requestHandler2 = TypeChecks.GetRequestHandler2(compilation);
            if (requestHandler2 is null)
                return;
            if (TypeChecks.TryExtractRequestHandler(symbol, requestHandler1, requestHandler2, out var handler))
            {
                Logger.Log("New handler detected {0} -> {1}", handler.Handler, handler.Request);
                Handlers.TryAdd(handler.Request, handler);
            }
        }
    }
}
