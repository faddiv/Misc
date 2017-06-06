"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes, IControllerProvider, auto } from "angular";
import { IScopeEx } from "../../typings/testInterfaces";

describe("$compile", () => {
    var ng: IAngularStatic;
    var myModule: IModule;
    beforeEach(function () {
        delete window.angular;
        publishExternalAPI();
        ng = window.angular;
        myModule = ng.module("myModule", []);
    });

    afterEach(() => {
        delete window.angular;
    });

    function makeInjectorWithDirectives(name: any, ...params: object[]) {
        var args = arguments;
        return createInjector(["ng", function ($compileProvider: ICompileProvider) {
            $compileProvider.directive.apply($compileProvider, args);
        }]);
    }

    //Directive Controllers
    describe("controllers", () => {

        it("can be attached to directives as functions", () => {
            var controllerInvoked;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    controller: function MyController() {
                        controllerInvoked = true;
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(controllerInvoked).toBe(true);
            });
        });

        it("can be attached to directives as string references", () => {
            var controllerInvoked;
            function MyController() {
                controllerInvoked = true;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(controllerInvoked).toBe(true);
            });
        });
        
        it("can be applied in the same element independent of each other", () => {
            var controllerInvoked;
            var otherControllerInvoked;
            function MyController() {
                controllerInvoked = true;
            }
            function MyOtherController() {
                otherControllerInvoked = true;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $controllerProvider.register("MyOtherController", MyOtherController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                    $compileProvider.directive("myOtherDirective", function () {
                        return {
                            controller: "MyOtherController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(controllerInvoked).toBe(true);
                expect(otherControllerInvoked).toBe(true);
            });
        });

        it("can be applied to different directives, as different instances", () => {
            var invocations = 0;;
            function MyController() {
                invocations++;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                    $compileProvider.directive("myOtherDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(invocations).toBe(2);
            });
        });
    });
});