"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes } from "angular";

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

    it("creates an isolate scope when requested", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {},
                link(scope: IScope, element: JQuery, attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div my-directive>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(givenScope.$parent).toBe($rootScope);
            expect(Object.getPrototypeOf(givenScope)).not.toBe($rootScope);
        });
    });

    it("does not share isolate scope with other directives", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives({
            firstDirective() {
                return {
                    scope: {}
                };
            },
            secondDirective() {
                return {
                    link(scope) {
                        givenScope = scope;
                    }
                };
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div first-directive second-directive></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(givenScope).toBe($rootScope);
        });
    });

    it("does not isolate scope on child elements", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives({
            firstDirective() {
                return {
                    scope: {}
                };
            },
            secondDirective() {
                return {
                    link(scope) {
                        givenScope = scope;
                    }
                };
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div first-directive><div second-directive></div></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(givenScope).toBe($rootScope);
        });
    });

    it("does not allow two isolate scope directives on an element", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives({
            firstDirective() {
                return {
                    scope: {}
                };
            },
            secondDirective() {
                return {
                    scope: {}
                };
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div first-directive second-directive></div>');
            expect(() => {
                $compile(el);
            }).toThrow();
        });
    });

    it("does not allow both isolate and inherited scopes on an element", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives({
            firstDirective() {
                return {
                    scope: {}
                };
            },
            secondDirective() {
                return {
                    scope: true
                };
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div first-directive second-directive></div>');
            expect(() => {
                $compile(el);
            }).toThrow();
        });
    });

    it("adds class and data for element with isolated scope", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {},
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div my-directive></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(el.hasClass("ng-isolate-scope")).toBe(true);
            expect(el.hasClass("ng-scope")).toBe(false);
            expect(el.data("$isolateScope")).toBe(givenScope);
            //expect(el.data("$scope")).toBeUndefined();
        });
    });

    //Isolate Attribute Bindings
    it("allows observing attribute to the isolate scope", () => {
        var givenScope: IScope;
        var givenAttrs: IAttributes;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    anAttr: "@"
                },
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                    givenAttrs = attrs;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div my-directive></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            givenAttrs.$set("anAttr", "42");
            expect(givenScope.anAttr).toEqual("42");
        });
    });

    it("sets initial value of observed attr to the isolate scope", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    anAttr: "@"
                },
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div my-directive an-attr="42"></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(givenScope.anAttr).toEqual("42");
        });
    });
    
    it("allows aliasing observed attribute", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    aScopeAttr: "@anAttr"
                },
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div my-directive an-attr="42"></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(givenScope.aScopeAttr).toEqual("42");
        });
    });
});