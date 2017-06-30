"use strict";
import * as _ from "lodash";
import * as sinon from 'sinon';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { auto, IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes, IComponentOptions, IControllerProvider } from "angular";

describe("components", () => {

    beforeEach(() => {
        publishExternalAPI();
    });

    afterEach(() => {
        delete window.angular;
    });

    function makeInjectorWithComponent(options: IComponentOptions): auto.IInjectorService;
    function makeInjectorWithComponent(name: string, options?: IComponentOptions): auto.IInjectorService;
    function makeInjectorWithComponent(name: string | IComponentOptions, options?: IComponentOptions): auto.IInjectorService {
        return createInjector(["ng", function ($compileProvider: ICompileProvider) {
            if (typeof (name) === "string") {
                $compileProvider.component(name, options);
            } else {
                $compileProvider.component("myComponent", name);
            }
        }]);
    }
    //Registering Components
    it("can be registered and became directives", () => {
        var myModule = window.angular.module("myModule", []);
        myModule.component("myComponent", {});
        var injector = createInjector(["ng", "myModule"]);
        expect(injector.has("myComponentDirective")).toBe(true);
    });

    //Basic Components
    it("are element directives with controllers", () => {
        var controllerInstantiated = false;
        var componentElement: JQuery;
        var injector = makeInjectorWithComponent({
            controller($element) {
                controllerInstantiated = true;
                componentElement = $element;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>");
            $compile(el)($rootScope);
            expect(controllerInstantiated).toBe(true);
            expect(el[0]).toBe(componentElement[0]);
        });
    });

    it("cannot be applied to an attribute", () => {
        var controllerInstantiated = false;
        var injector = makeInjectorWithComponent(<any>{
            restrict: "A", //Will be ignorred
            controller() {
                controllerInstantiated = true;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-component></div>");
            $compile(el)($rootScope);
            expect(controllerInstantiated).toBe(false);
        });
    });

    //Component Scopes and Bindings
    it("has an isolate scope", () => {
        var componentScope: IScope;
        var injector = makeInjectorWithComponent({
            controller($scope) {
                componentScope = $scope;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>");
            $compile(el)($rootScope);
            expect(componentScope).not.toBe($rootScope);
            expect(componentScope.$parent).toBe($rootScope);
            expect(Object.getPrototypeOf(componentScope)).not.toBe($rootScope);
        });
    });

    it("may have bindings which are attached to controller", () => {
        var controllerInstance;
        var injector = makeInjectorWithComponent({
            bindings: {
                attr: "@",
                oneWay: "<",
                twoWay: "="
            },
            controller() {
                controllerInstance = this;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            $rootScope.b = 42;
            $rootScope.c = 43;
            var el = $("<my-component attr='a' one-way='b' two-way='c'></my-component>")
            $compile(el)($rootScope);

            expect(controllerInstance.attr).toEqual("a");
            expect(controllerInstance.oneWay).toEqual(42);
            expect(controllerInstance.twoWay).toEqual(43);
        });
    });

    it("may use a controller alias with controllerAs", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = makeInjectorWithComponent({
            controller($scope) {
                componentScope = $scope;
                controllerInstance = this;
            },
            controllerAs: "ctrl"
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            $rootScope.b = 42;
            $rootScope.c = 43;
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);

            expect(componentScope.ctrl).toEqual(controllerInstance);
        });
    });

    it("may use a controller alias with 'controller as' syntax", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
            $controllerProvider.register("MyController", function ($scope) {
                componentScope = $scope;
                controllerInstance = this;
            });
            $compileProvider.component("myComponent", {
                controller: "MyController as ctrl"
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);

            expect(componentScope.ctrl).toEqual(controllerInstance);
        });
    });

    it("has a default controller alias of $ctrl", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
            $controllerProvider.register("MyController", function ($scope) {
                componentScope = $scope;
                controllerInstance = this;
            });
            $compileProvider.component("myComponent", {
                controller: "MyController"
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);

            expect(componentScope.$ctrl).toEqual(controllerInstance);
        });
    });

    //Component Templates
});