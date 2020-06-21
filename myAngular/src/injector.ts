import * as _ from 'lodash';
import { HashMap } from './hash_map';
import { IServiceProvider, IProviderCache, IInjectorCache, IModuleInternal } from "./angularInterfaces";
import { auto, Injectable } from "angular";

export function createInjector(modulesToLoad: Injectable<Function>, strictDi?: boolean): auto.IInjectorService {
    var providerCache: IProviderCache = {};
    var providerInjector = providerCache.$injector = createInternalInjector(providerCache, function () {
        throw "Unknown provider: " + path.join(" <- ");
    });
    var instanceCache: IInjectorCache = {};
    var instanceInjector: auto.IInjectorService = instanceCache.$injector = createInternalInjector(instanceCache, function (name: string) {
        var provider = providerInjector.get<IServiceProvider>(name + "Provider");
        return instanceInjector.invoke(provider.$get, provider);
    });
    var loadedModules = new HashMap();
    var path: string[] = [];
    const FN_ARGS = /^function\s*[^\(]*\(\s*([^\)]*)\)/m;
    const FN_ARG = /^\s*(_?)(\S+?)\1\s*$/;
    const STRIP_COMMENTS = /(\/\/.*$)|(\/\*.*?\*\/)/mg;
    const INSTANTIATING = {};
    strictDi = (strictDi === true);

    function enforceReturnValue(factoryFn: Function | any[]) {
        return function () {
            var value = instanceInjector.invoke(<any>factoryFn);
            if (_.isUndefined(value)) {
                throw "factory must return a value";
            }
            return value;
        };
    }

    providerCache.$provide = <auto.IProvideService>{
        constant(key: string, value: any): void {
            if (key === "hasOwnProperty") {
                throw "hasOwnProperty is not a valid constant name";
            }
            providerCache[key] = value;
            instanceCache[key] = value;
        },
        provider(key: string, providerOrFunction: IServiceProvider | Function): IServiceProvider {
            var provider: IServiceProvider;
            if (_.isFunction(providerOrFunction)) {
                provider = providerInjector.instantiate<IServiceProvider>(providerOrFunction);
            } else {
                provider = providerOrFunction;
            }
            providerCache[key + "Provider"] = provider;
            return provider;
        },
        factory(key: string, factoryFn: Function | any[], enforce): IServiceProvider {
            return this.provider(key, {
                $get: enforce === false ? factoryFn : enforceReturnValue(factoryFn)
            });
        },
        decorator(serviceName: string, decoratorFn: Function): void {
            var provider = providerInjector.get<IServiceProvider>(serviceName + "Provider");
            var originalGet = provider.$get;
            provider.$get = function () {
                var instance = instanceInjector.invoke(originalGet, provider);
                instanceInjector.invoke(decoratorFn, null, { $delegate: instance });
                return instance;
            }
        },
        service(key: string, constructor: Function): IServiceProvider {
            return this.factory(key, function () {
                return instanceInjector.instantiate(constructor);
            });
        },
        value(key: string, value: any): IServiceProvider {
            return this.factory(key, _.constant(value), false);
        }
    };

    function createInternalInjector(cache, factoryFn: (name?: string) => any): auto.IInjectorService {

        function getService(name: string): any {
            if (cache.hasOwnProperty(name)) {
                if (cache[name] === INSTANTIATING) {
                    var circularDependency = name + " <- " + path.join(" <- ");
                    throw new Error("Circular dependency found at " + circularDependency);
                }
                return cache[name];
            } else {
                path.unshift(name);
                cache[name] = INSTANTIATING;
                try {
                    cache[name] = factoryFn(name);
                    return cache[name];
                } finally {
                    path.shift();
                    if (cache[name] === INSTANTIATING) {
                        delete cache[name];
                    }
                }
            }
        }

        function invoke(func: Function, context?: any, locals?: any): any;
        function invoke(inlineAnnotatedFunction: any[]): any;
        function invoke(fn: any[] | Function, context?: any, locals?: any): any {
            var args = _.map(annotate(<any>fn), token => {
                if (_.isString(token)) {
                    return locals && locals.hasOwnProperty(token) ? locals[token] : getService(token);
                } else {
                    throw "incorrect injection token! Expected a string, got " + token;
                }
            });
            if (_.isArray(fn)) {
                return (<Function>_.last(fn)).apply(context, args);
            } else if (_.isFunction(fn)) {
                return fn.apply(context, args);
            }
        }

        function instantiate<T>(typeConstructor: Function, locals?: any): T;
        function instantiate<T>(inlineAnnotatedFunction: any[]): T;
        function instantiate(typeConstructor: Function | any[], locals?: any) {
            var unwrappedType: Function = _.isArray(typeConstructor)
                ? _.last(typeConstructor)
                : <any>typeConstructor;
            var instance = Object.create(unwrappedType.prototype);
            invoke(<any>typeConstructor, instance, locals);
            return instance;
        }

        return <auto.IInjectorService>{
            has(key: string): boolean {
                return cache.hasOwnProperty(key) ||
                    providerCache.hasOwnProperty(key + "Provider");
            },
            get: getService,
            invoke: invoke,
            annotate: annotate,
            instantiate: instantiate,
            strictDi: false,
        };
    }

    function annotate(fn: Function, strictDi?: boolean): string[];
    function annotate(inlineAnnotatedFunction: any[]): string[];
    function annotate(fn: any[] | Function, strictDi2?: boolean): string[] {
        strictDi2 = (strictDi2 === true) || strictDi;
        if (_.isArray(fn)) {
            return <string[]>fn.slice(0, fn.length - 1);
        } else if (_.isFunction(fn)) {
            if (fn.$inject) {
                return <string[]>fn.$inject;
            } else if (!fn.length) {
                return [];
            } else if (strictDi2) {
                throw "fn is not using explicit annotation and cannot be invoked in strict mode";
            } else {
                var source = fn.toString().replace(STRIP_COMMENTS, "");
                var argDeclaration = source.match(FN_ARGS);
                return _.map(argDeclaration[1].split(','), argName => {
                    return argName.match(FN_ARG)[2];
                });
            }
        }
    }

    function runInvokeQueue(queue: any[]) {
        _.forEach(queue, invokeArgs => {
            var service = providerInjector.get(invokeArgs[0]);
            var method = invokeArgs[1];
            var args = invokeArgs[2];
            service[method].apply(service, args);
        });
    }
    var runBlocks: Array<string> = [];
    _.forEach(modulesToLoad, function loadModule(moduleName: Injectable<Function>) {
        if (!loadedModules.get(moduleName)) {
            loadedModules.put(moduleName, true);
            if (_.isString(moduleName)) {
                var module = <IModuleInternal>window.angular.module(moduleName);
                _.forEach(module.requires, loadModule);
                runInvokeQueue(module._invokeQueue);
                runInvokeQueue(module._configBlocks);
                runBlocks = runBlocks.concat(<any[]>module._runBlocks);
            } else if (_.isFunction(moduleName)) {
                runBlocks.push(providerInjector.invoke(moduleName));
            } else if (_.isArray(moduleName)) {
                runBlocks.push(providerInjector.invoke(moduleName));
            }
        }
    });
    _.forEach(_.compact(runBlocks), runBlock => {
        if (_.isFunction(runBlock)) {
            instanceInjector.invoke(runBlock);
        }
    });

    return instanceInjector;
}