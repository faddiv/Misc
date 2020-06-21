"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IControllerService, auto, IControllerProvider, ICompileProvider, ICompileService, IScope, IAttributes } from "angular";
import $ from "jquery";

describe("ngController", () => {
    beforeEach(function () {
        publishExternalAPI();
    });

    afterEach(() => {
        delete window.angular;
    });

    it("is instantiated during compilation & linking", () => {
        var instantiated;
        class MyController {
            constructor() {
                instantiated = true;
            }
        }
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider) {
            $controllerProvider.register("MyController", MyController);
        }]);
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div ng-controller='MyController'>");
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(instantiated).toBe(true);
        });
    });

    it("may inject scope, element, and attrs", () => {
        var gotScope: IScope;
        var gotElement: JQuery;
        var gotAttrs: IAttributes;
        class MyController {
            constructor($scope, $element, $attrs) {
                gotScope = $scope;
                gotElement = $element;
                gotAttrs = $attrs;
            }
        }
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider) {
            $controllerProvider.register("MyController", MyController);
        }]);
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div ng-controller='MyController'>");
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(gotScope).toBeDefined();
            expect(gotElement).toBeDefined();
            expect(gotAttrs).toBeDefined();
        });
    });

    it("has an inherited scope", () => {
        var gotScope: IScope;
        class MyController {
            constructor($scope) {
                gotScope = $scope;
            }
        }
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider) {
            $controllerProvider.register("MyController", MyController);
        }]);
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div ng-controller='MyController'>");
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(gotScope).not.toBe($rootScope);
            expect(gotScope.$parent).toBe($rootScope);
            expect(Object.getPrototypeOf(gotScope)).toBe($rootScope);
        });
    });

    //Attaching Controllers on The Scope
    it("allow aliasing controller in expression", () => {
        var gotScope: IScope;
        class MyController {
            constructor($scope) {
                gotScope = $scope;
            }
        }
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider) {
            $controllerProvider.register("MyController", MyController);
        }]);
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div ng-controller='MyController as ctrl'>");
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(gotScope.ctrl).toBeDefined();
            expect(gotScope.ctrl instanceof MyController).toBe(true);
        });
    });


});