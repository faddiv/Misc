// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using AgileObjects.ReadableExpressions;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal sealed class ExpressionResolverBuilder : CallSiteVisitor<object?, Expression>
    {
        private static readonly ParameterExpression ScopeParameter = Expression.Parameter(typeof(ServiceProviderEngineScope));

        private static readonly ParameterExpression ResolvedServices = Expression.Variable(typeof(IDictionary<ServiceCacheKey, object>), ScopeParameter.Name + "resolvedServices");
        private static readonly ParameterExpression Sync = Expression.Variable(typeof(object), ScopeParameter.Name + "sync");
        private static readonly BinaryExpression ResolvedServicesVariableAssignment =
            Expression.Assign(ResolvedServices,
                Expression.Property(
                    ScopeParameter,
                    typeof(ServiceProviderEngineScope).GetProperty(nameof(ServiceProviderEngineScope.ResolvedServices), BindingFlags.Instance | BindingFlags.NonPublic)!));

        private static readonly BinaryExpression SyncVariableAssignment =
            Expression.Assign(Sync,
                Expression.Property(
                    ScopeParameter,
                    typeof(ServiceProviderEngineScope).GetProperty(nameof(ServiceProviderEngineScope.Sync), BindingFlags.Instance | BindingFlags.NonPublic)!));

        private static readonly ParameterExpression CaptureDisposableParameter = Expression.Parameter(typeof(object));
        private static readonly LambdaExpression CaptureDisposable = Expression.Lambda(
                    Expression.Call(ScopeParameter, ServiceLookupHelpers.CaptureDisposableMethodInfo, CaptureDisposableParameter),
                    CaptureDisposableParameter);

        private static readonly ConstantExpression CallSiteRuntimeResolverInstanceExpression = Expression.Constant(
            CallSiteRuntimeResolver.Instance,
            typeof(CallSiteRuntimeResolver));

        private readonly ServiceProviderEngineScope _rootScope;

        private readonly ConcurrentDictionary<ServiceCacheKey, Func<ServiceProviderEngineScope, object>> _scopeResolverCache;

        private readonly Func<ServiceCacheKey, ServiceCallSite, Func<ServiceProviderEngineScope, object>> _buildTypeDelegate;

        public ExpressionResolverBuilder(ServiceProvider serviceProvider)
        {
            _rootScope = serviceProvider.Root;
            _scopeResolverCache = new ConcurrentDictionary<ServiceCacheKey, Func<ServiceProviderEngineScope, object>>();
            _buildTypeDelegate = (key, cs) => BuildNoCache(cs);
        }

        public Func<ServiceProviderEngineScope, object> Build(ServiceCallSite callSite)
        {
            // Only scope methods are cached
            if (callSite.Cache.Location == CallSiteResultCacheLocation.Scope)
            {
#if NETFRAMEWORK || NETSTANDARD2_0
                return _scopeResolverCache.GetOrAdd(callSite.Cache.Key, key => _buildTypeDelegate(key, callSite));
#else
                return _scopeResolverCache.GetOrAdd(callSite.Cache.Key, _buildTypeDelegate, callSite);
#endif
            }

            return BuildNoCache(callSite);
        }

        public Func<ServiceProviderEngineScope, object> BuildNoCache(ServiceCallSite callSite)
        {
            Expression<Func<ServiceProviderEngineScope, object>> expression = BuildExpression(callSite);
            DependencyInjectionEventSource.Log.ExpressionTreeGenerated(_rootScope.RootProvider, callSite.ServiceType, expression);
            Console.WriteLine(expression.ToReadableString());
            return expression.Compile();
        }

        private Expression<Func<ServiceProviderEngineScope, object>> BuildExpression(ServiceCallSite callSite)
        {
            if (callSite.Cache.Location == CallSiteResultCacheLocation.Scope)
            {
                return Expression.Lambda<Func<ServiceProviderEngineScope, object>>(
                    Expression.Block(
                        new[] { ResolvedServices, Sync },
                        ResolvedServicesVariableAssignment,
                        SyncVariableAssignment,
                        BuildScopedExpression(callSite)),
                    ScopeParameter);
            }

            return Expression.Lambda<Func<ServiceProviderEngineScope, object>>(
                Convert(VisitCallSite(callSite, null), typeof(object), forceValueTypeConversion: true),
                ScopeParameter);
        }

        protected override Expression VisitRootCache(ServiceCallSite singletonCallSite, object? context)
        {
            return Expression.Constant(CallSiteRuntimeResolver.Instance.Resolve(singletonCallSite, _rootScope));
        }

        protected override Expression VisitConstant(ConstantCallSite constantCallSite, object? context)
        {
            return Expression.Constant(constantCallSite.DefaultValue);
        }

        protected override Expression VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, object? context)
        {
            return ScopeParameter;
        }

        protected override Expression VisitFactory(FactoryCallSite factoryCallSite, object? context)
        {
            return Expression.Invoke(Expression.Constant(factoryCallSite.Factory), ScopeParameter);
        }

        protected override Expression VisitIEnumerable(IEnumerableCallSite callSite, object? context)
        {
            [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
                Justification = "VerifyAotCompatibility ensures elementType is not a ValueType")]
            static MethodInfo GetArrayEmptyMethodInfo(Type elementType)
            {
                Debug.Assert(!ServiceProvider.VerifyAotCompatibility || !elementType.IsValueType, "VerifyAotCompatibility=true will throw during building the IEnumerableCallSite if elementType is a ValueType.");

                return ServiceLookupHelpers.GetArrayEmptyMethodInfo(elementType);
            }

            [UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
                Justification = "VerifyAotCompatibility ensures elementType is not a ValueType")]
            static NewArrayExpression NewArrayInit(Type elementType, IEnumerable<Expression> expr)
            {
                Debug.Assert(!ServiceProvider.VerifyAotCompatibility || !elementType.IsValueType, "VerifyAotCompatibility=true will throw during building the IEnumerableCallSite if elementType is a ValueType.");

                return Expression.NewArrayInit(elementType, expr);
            }

            if (callSite.ServiceCallSites.Length == 0)
            {
                return Expression.Constant(
                    GetArrayEmptyMethodInfo(callSite.ItemType)
                    .Invoke(obj: null, parameters: Array.Empty<object>()));
            }

            return NewArrayInit(
                callSite.ItemType,
                callSite.ServiceCallSites.Select(cs =>
                    Convert(
                        VisitCallSite(cs, context),
                        callSite.ItemType)));
        }

        protected override Expression VisitDisposeCache(ServiceCallSite callSite, object? context)
        {
            // Elide calls to GetCaptureDisposable if the implementation type isn't disposable
            return TryCaptureDisposable(
                callSite,
                ScopeParameter,
                VisitCallSiteMain(callSite, context));
        }

        private static Expression TryCaptureDisposable(ServiceCallSite callSite, ParameterExpression scope, Expression service)
        {
            if (!callSite.CaptureDisposable)
            {
                return service;
            }

            return Expression.Invoke(GetCaptureDisposable(scope), service);
        }

        protected override Expression VisitFactoryClass(
            FactoryClassCallSite callSite,
            object? context)
        {
            var factoryConstant = Expression.Constant(callSite.Factory);
            var createInstanceMethod = typeof(FactoryClassWrapper)
                .GetMethod(nameof(FactoryClassWrapper.CreateInstance))!;
            var parameterTypes = callSite.Factory.ParameterTypes;
            Expression[] parameterExpressions;
            if (callSite.ParameterCallSites.Length == 0)
            {
                parameterExpressions = [];
            }
            else
            {
                parameterExpressions = new Expression[callSite.ParameterCallSites.Length];
                for (int i = 0; i < parameterExpressions.Length; i++)
                {
                    var visitCallSite = VisitCallSite(callSite.ParameterCallSites[i], context);
                    visitCallSite = Convert(visitCallSite, parameterTypes[i]);
                    parameterExpressions[i] = visitCallSite;
                }
            }

            var newArrayExpression = Expression.NewArrayInit(typeof(object), parameterExpressions);

            Expression expression = Expression.Call(createInstanceMethod, factoryConstant, newArrayExpression);

            return expression;
        }

        protected override Expression VisitConstructor(ConstructorCallSite callSite, object? context)
        {
            ParameterInfo[] parameters = callSite.ConstructorInfo.GetParameters();
            Expression[] parameterExpressions;
            if (callSite.ParameterCallSites.Length == 0)
            {
                parameterExpressions = Array.Empty<Expression>();
            }
            else
            {
                parameterExpressions = new Expression[callSite.ParameterCallSites.Length];
                for (int i = 0; i < parameterExpressions.Length; i++)
                {
                    var parameterType = parameters[i].ParameterType;
                    var visitCallSite = VisitCallSite(callSite.ParameterCallSites[i], context);
                    parameterExpressions[i] = Convert(visitCallSite, parameterType);
                }
            }

            Expression expression = Expression.New(callSite.ConstructorInfo, parameterExpressions);
            if (callSite.ImplementationType!.IsValueType)
            {
                expression = Expression.Convert(expression, typeof(object));
            }
            return expression;
        }

        private static Expression Convert(Expression expression, Type type, bool forceValueTypeConversion = false)
        {
            // Don't convert if the expression is already assignable
            if (type.IsAssignableFrom(expression.Type)
                && (!expression.Type.IsValueType || !forceValueTypeConversion))
            {
                return expression;
            }

            return Expression.Convert(expression, type);
        }

        protected override Expression VisitScopeCache(ServiceCallSite callSite, object? context)
        {
            Func<ServiceProviderEngineScope, object> lambda = Build(callSite);
            return Expression.Invoke(Expression.Constant(lambda), ScopeParameter);
        }

        // Move off the main stack
        private ConditionalExpression BuildScopedExpression(ServiceCallSite callSite)
        {
            ConstantExpression callSiteExpression = Expression.Constant(
                callSite,
                typeof(ServiceCallSite));

            // We want to directly use the callsite value if it's set and the scope is the root scope.
            // We've already called into the RuntimeResolver and pre-computed any singletons or root scope
            // Avoid the compilation for singletons (or promoted singletons)
            MethodCallExpression resolveRootScopeExpression = Expression.Call(
                CallSiteRuntimeResolverInstanceExpression,
                ServiceLookupHelpers.ResolveCallSiteAndScopeMethodInfo,
                callSiteExpression,
                ScopeParameter);

            ConstantExpression keyExpression = Expression.Constant(
                callSite.Cache.Key,
                typeof(ServiceCacheKey));

            ParameterExpression resolvedVariable = Expression.Variable(typeof(object), "resolved");

            ParameterExpression resolvedServices = ResolvedServices;

            MethodCallExpression tryGetValueExpression = Expression.Call(
                resolvedServices,
                ServiceLookupHelpers.TryGetValueMethodInfo,
                keyExpression,
                resolvedVariable);

            Expression captureDisposible = TryCaptureDisposable(callSite, ScopeParameter, VisitCallSiteMain(callSite, null));

            BinaryExpression assignExpression = Expression.Assign(
                resolvedVariable,
                captureDisposible);

            MethodCallExpression addValueExpression = Expression.Call(
                resolvedServices,
                ServiceLookupHelpers.AddMethodInfo,
                keyExpression,
                resolvedVariable);

            BlockExpression blockExpression = Expression.Block(
                typeof(object),
                new[]
                {
                    resolvedVariable
                },
                Expression.IfThen(
                    Expression.Not(tryGetValueExpression),
                    Expression.Block(
                        assignExpression,
                        addValueExpression)),
                resolvedVariable);


            // The C# compiler would copy the lock object to guard against mutation.
            // We don't, since we know the lock object is readonly.
            ParameterExpression lockWasTaken = Expression.Variable(typeof(bool), "lockWasTaken");
            ParameterExpression sync = Sync;

            MethodCallExpression monitorEnter = Expression.Call(ServiceLookupHelpers.MonitorEnterMethodInfo, sync, lockWasTaken);
            MethodCallExpression monitorExit = Expression.Call(ServiceLookupHelpers.MonitorExitMethodInfo, sync);

            BlockExpression tryBody = Expression.Block(monitorEnter, blockExpression);
            ConditionalExpression finallyBody = Expression.IfThen(lockWasTaken, monitorExit);

            return Expression.Condition(
                    Expression.Property(
                        ScopeParameter,
                        typeof(ServiceProviderEngineScope)
                            .GetProperty(nameof(ServiceProviderEngineScope.IsRootScope), BindingFlags.Instance | BindingFlags.Public)!),
                    resolveRootScopeExpression,
                    Expression.Block(
                        typeof(object),
                        new[] { lockWasTaken },
                        Expression.TryFinally(tryBody, finallyBody))
                );
        }

        public static Expression GetCaptureDisposable(ParameterExpression scope)
        {
            if (scope != ScopeParameter)
            {
                throw new NotSupportedException(SR.GetCaptureDisposableNotSupported);
            }
            return CaptureDisposable;
        }
    }
}
