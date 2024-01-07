using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MediatR.Analyzers.Utilities
{
    public class DiagnosticDataCache
    {

        private static DiagnosticDataCache _cache;
        private static bool _cacheInitialized = false;
        private static object _cacheLock = new object();

        public static DiagnosticDataCache GetInstance(Compilation compilation)
        {
            if(_cacheInitialized)
            {
                return _cache;
            }
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
            if (HasHandlerFromCache(request, compilation))
            {
                return true;
            }
            //Maybe new appeared.
            var requestHandler1 = TypeChecks.GetRequestHandler1(compilation);
            if (requestHandler1 is null)
                return false;
            var requestHandler2 = TypeChecks.GetRequestHandler2(compilation);
            if (requestHandler2 is null)
                return false;
            var collector = new HandlerCollectorSymbolVisitor(requestHandler1, requestHandler2);
            compilation.Assembly.Accept(collector);
            foreach (var handler in collector.CollectedHandlers)
            {
                TryCacheHandler(handler);
            }
            return HasHandlerFromCache(request, compilation);
        }

        private bool HasHandlerFromCache(INamedTypeSymbol request, Compilation compilation)
        {
            var requestName = request.ToDisplayString();
            if (Handlers.TryGetValue(requestName, out var foundHandlerInfo))
            {
                var handler = compilation.GetTypeByMetadataName(foundHandlerInfo.Handler);
                if (!(handler is null))
                {
                    return true;
                }
                else
                {
                    Logger.Log("Handler dissapeared {0} -> {1}", foundHandlerInfo.Handler, foundHandlerInfo.Request);
                    Handlers.TryRemove(requestName, out _);
                }
            }
            return false;
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
                TryCacheHandler(handler);
            }
        }

        private bool TryCacheHandler(HandlerInfo handler)
        {
            bool added = Handlers.TryAdd(handler.Request, handler);
            if (added)
            {
                Logger.Log("New handler detected {0} -> {1}", handler.Handler, handler.Request);
            }
            return added;
        }
    }
}
