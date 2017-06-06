import { IModuleContainer, IModuleInternal } from "./angularInterfaces";
import { IModule, Injectable } from "angular";

"use strict";

export function setupModuleLoader(w: Window) {
    var ensure = function (obj, name, factory) {
        return obj[name] || (obj[name] = factory());
    }
    var angular = ensure(w, "angular", Object);

    var createModule = function (name: string, requires: string[], modules: IModuleContainer, configFn: Function): IModule {
        if (name === "hasOwnProperty") {
            throw "hasOwnProperty is not a valid module name";
        }
        var invokeQueue = [];
        var configBlocks = [];

        var invokeLater = function (service: string, method: string, arrayMethod: string, queue: any[]) {
            return function () {
                var item = [service, method, arguments];
                queue[arrayMethod](item);
                return moduleInstance;
            };
        };
        var moduleInstance = <IModuleInternal>{
            name: name,
            requires: requires,
            constant: invokeLater("$provide", "constant", "unshift", invokeQueue),
            provider: invokeLater("$provide", "provider", "push", invokeQueue),
            factory: invokeLater("$provide", "factory", "push", invokeQueue),
            value: invokeLater("$provide", "value", "push", invokeQueue),
            service: invokeLater("$provide", "service", "push", invokeQueue),
            decorator: invokeLater("$provide", "decorator", "push", invokeQueue),
            filter: invokeLater("$filterProvider", "register", "push", invokeQueue),
            directive: invokeLater("$compileProvider", "directive", "push", invokeQueue),
            controller: invokeLater("$controllerProvider", "register", "push", invokeQueue),
            config: invokeLater("$injector", "invoke", "push", configBlocks),
            run(fn: Injectable<Function>) {
                moduleInstance._runBlocks.push(fn);
                return moduleInstance;
            },
            _invokeQueue: invokeQueue,
            _configBlocks: configBlocks,
            component: undefined,
            _runBlocks: []
        };
        if (configFn) {
            moduleInstance.config(configFn);
        }
        modules[name] = moduleInstance;
        return moduleInstance;
    }
    var getModule = function (name: string, modules: IModuleContainer): IModuleInternal {
        if (modules.hasOwnProperty(name)) {
            var moduleInstance = modules[name];
            return moduleInstance;
        } else {
            throw "Module " + name + "is not available!";
        }
    }
    ensure(angular, "module", function () {
        var modules: IModuleContainer = {};
        return function (name: string, requires?: string[], configFn?: Function) {
            if (requires) {
                return createModule(name, requires, modules, configFn);
            } else {
                return getModule(name, modules);
            }
        };
    });
}