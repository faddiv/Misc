"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes } from "angular";
import $ from "jquery";

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
    describe("interpolation", () => {
        it("is done for text nodes", () => {
            var injector = makeInjectorWithDirectives({

            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div>expr: {{expr}}</div>");
                $compile(el)($rootScope);

                $rootScope.$apply();
                expect(el.html()).toEqual("expr: ");

                $rootScope.expr = "Hello";
                $rootScope.$apply();
                expect(el.html()).toEqual("expr: Hello");
            });
        });

        it("adds binding class to text node parents", () => {
            var injector = makeInjectorWithDirectives({

            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div>expr: {{expr}}</div>");
                $compile(el)($rootScope);

                expect(el.hasClass("ng-binding")).toBe(true);
            });
        });

        it("adds binding data to text node parents", () => {
            var injector = makeInjectorWithDirectives({

            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div>exprs: {{expr1}} {{expr2}}</div>");
                $compile(el)($rootScope);

                expect(el.data("$binding")).toEqual(["expr1", "expr2"]);
            });
        });

        it("adds binding data to parent from multiple text nodes", () => {
            var injector = makeInjectorWithDirectives({

            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div>exprs: {{expr1}} <span>and</span> {{expr2}}</div>");
                $compile(el)($rootScope);

                expect(el.data("$binding")).toEqual(["expr1", "expr2"]);
            });
        });

        //Attribute Interpolation
        it("is done for attributes", () => {
            var injector = makeInjectorWithDirectives({

            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<img alt='{{expr}}'>");
                $compile(el)($rootScope);
                $rootScope.$apply();

                expect(el.attr("alt")).toEqual("");

                $rootScope.expr = "picture";
                $rootScope.$apply();

                expect(el.attr("alt")).toEqual("picture");
            });
        });

        it("fires observers on attribute expression changes", () => {
            var observerSpy = jasmine.createSpy("observerSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        link(scope: IScope, element: JQuery, attrs: IAttributes) {
                            attrs.$observe("alt", observerSpy);
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<img alt='{{expr}}' my-directive>");
                $compile(el)($rootScope);

                $rootScope.expr = "picture";
                $rootScope.$apply();

                expect(observerSpy.calls.mostRecent().args[0]).toEqual("picture");
            });
        });

        it("fires observers just once upon registration", () => {
            var observerSpy = jasmine.createSpy("observerSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        link(scope: IScope, element: JQuery, attrs: IAttributes) {
                            attrs.$observe("alt", observerSpy);
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<img alt='{{expr}}' my-directive>");
                $compile(el)($rootScope);
                $rootScope.$apply();

                expect(observerSpy.calls.count()).toBe(1);
                expect(observerSpy.calls.mostRecent().args[0]).toBe("");
            });
        });

        it("is done for attributes by the time other directive is linked", () => {
            var gotMyAttr;
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        link(scope: IScope, element: JQuery, attrs: IAttributes) {
                            gotMyAttr = attrs.myAttr;
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<img my-attr='{{expr}}' my-directive>");
                $rootScope.expr = "Hello";
                $compile(el)($rootScope);

                expect(gotMyAttr).toBe("Hello");
            });
        });

        it("is done for attributes by the time bound to isolate scope", () => {
            var gotMyAttr;
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        scope: { myAttr: "@" },
                        link(scope: IScope, element: JQuery, attrs: IAttributes) {
                            gotMyAttr = attrs.myAttr;
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<img my-attr='{{expr}}' my-directive>");
                $rootScope.expr = "Hello";
                $compile(el)($rootScope);

                expect(gotMyAttr).toBe("Hello");
            });
        });

        it("is done for attributes so that compile-time changes apply", () => {
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        compile(element: JQuery, attrs: IAttributes) {
                            attrs.$set("myAttr", "{{expr2}}");
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<img my-attr='{{expr1}}' my-directive>");
                $rootScope.expr1 = "Hello";
                $rootScope.expr2 = "Chiao";
                $compile(el)($rootScope);
                $rootScope.$apply();

                expect(el.attr("my-attr")).toBe("Chiao");
            });
        });

        it("is done for attributes so that compile-time removal apply", () => {
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        compile(element: JQuery, attrs: IAttributes) {
                            attrs.$set("myAttr", null);
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<img my-attr='{{expr1}}' my-directive>");
                $rootScope.expr1 = "Hello";
                $rootScope.expr2 = "Chiao";
                $compile(el)($rootScope);
                $rootScope.$apply();

                expect(el.attr("my-attr")).toBeFalsy();
            });
        });

        it("cannot be done for event handler attributes on", () => {
            var injector = makeInjectorWithDirectives({

            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<button onclick='{{myFunction()}}'>ok</button>");
                $rootScope.myFunction = function () { };

                expect(function () {
                    $compile(el)($rootScope);
                }).toThrow();
            });
        });
        
        it("cannot be done for event handler attributes formaction", () => {
            var injector = makeInjectorWithDirectives({

            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<button formaction='{{myFunction()}}'>ok</button>");
                $rootScope.myFunction = function () { };

                expect(function () {
                    $compile(el)($rootScope);
                }).toThrow();
            });
        });
    });
});
