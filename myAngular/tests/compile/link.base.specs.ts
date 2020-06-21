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

    it("returns a public link function from compile", () => {
        var injector = makeInjectorWithDirectives(function () {
            return { compile: _.noop() };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $('<div my-directive>');
            var linkFn = $compile(el);
            expect(linkFn).toBeDefined();
        });
    });

    describe("linking", () => {

        it("takes a scope and attaches it to elements", () => {
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return { compile: _.noop() };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(el.data("$scope")).toBe($rootScope);
            });
        });

        it("calls directive link function with scope", () => {
            var givenScope: IScope;
            var givenElement: JQuery;
            var givenAttrs: IAttributes;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    compile() {
                        return function link(scope: IScope, element: JQuery, attrs: IAttributes) {
                            givenScope = scope;
                            givenElement = element;
                            givenAttrs = attrs;
                        };
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenScope).toBe($rootScope);
                expect(givenElement[0]).toBe(el[0]);
                expect(givenAttrs).toBeDefined();
                expect(givenAttrs.myDirective).toBeDefined();
            });
        });

        //Plain Directive Link Functions
        it("supporrts link function in directive definition object", () => {
            var givenScope: IScope;
            var givenElement: JQuery;
            var givenAttrs: IAttributes;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    link(scope: IScope, element: JQuery, attrs: IAttributes) {
                        givenScope = scope;
                        givenElement = element;
                        givenAttrs = attrs;
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenScope).toBe($rootScope);
                expect(givenElement[0]).toBe(el[0]);
                expect(givenAttrs).toBeDefined();
                expect(givenAttrs.myDirective).toBeDefined();
            });
        });
        //Linking Child Nodes
        it("links directive on child elements first", () => {
            var givenElements = [];
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    link(scope: IScope, element: JQuery, attrs: IAttributes) {
                        givenElements.push(element);
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive><div my-directive></div></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenElements.length).toBe(2);
                expect(givenElements[0][0]).toBe(el[0].firstChild);
                expect(givenElements[1][0]).toBe(el[0]);
            });
        });
        it("links children when parent has no directives", () => {
            var givenElements = [];
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    link(scope: IScope, element: JQuery, attrs: IAttributes) {
                        givenElements.push(element);
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div><div my-directive></div></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenElements.length).toBe(1);
                expect(givenElements[0][0]).toBe(el[0].firstChild);
            });
        });
        //Pre- And Post-Linking
        it("supporrts link function objects", () => {
            var linked = false;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    link: {
                        post(scope: IScope, element: JQuery, attrs: IAttributes) {
                            linked = true;
                        }
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div><div my-directive></div></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(linked).toBe(true);
            });
        });
        it("supports prelinking and postlinking", () => {
            var linkings: [string, Element][] = [];
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    link: {
                        pre(scope: IScope, element: JQuery, attrs: IAttributes) {
                            linkings.push(["pre", element[0]]);
                        },
                        post(scope: IScope, element: JQuery, attrs: IAttributes) {
                            linkings.push(["post", element[0]]);
                        }
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive><div my-directive></div></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(linkings.length).toBe(4);
                expect(linkings[0]).toEqual(["pre", el[0]]);
                expect(linkings[1]).toEqual(["pre", el[0].firstChild]);
                expect(linkings[2]).toEqual(["post", el[0].firstChild]);
                expect(linkings[3]).toEqual(["post", el[0]]);
            });
        });
        it("reverses priority for postlink functions", () => {
            var linkings: string[] = [];
            var injector = makeInjectorWithDirectives({
                firstDirective() {
                    return {
                        priority: 2,
                        link: {
                            pre(scope: IScope, element: JQuery, attrs: IAttributes) {
                                linkings.push("first-pre");
                            },
                            post(scope: IScope, element: JQuery, attrs: IAttributes) {
                                linkings.push("first-post");
                            }
                        }
                    };
                },
                secondDirective() {
                    return {
                        priority: 1,
                        link: {
                            pre(scope: IScope, element: JQuery, attrs: IAttributes) {
                                linkings.push("second-pre");
                            },
                            post(scope: IScope, element: JQuery, attrs: IAttributes) {
                                linkings.push("second-post");
                            }
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div first-directive second-directive></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(linkings).toEqual([
                    "first-pre",
                    "second-pre",
                    "second-post",
                    "first-post"]);
            });
        });
        //Keeping The Node List Stable for Linking
        it("stabilizes node list during linking", () => {
            var givenElements = [];
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    link(scope: IScope, element: JQuery, attrs: IAttributes) {
                        givenElements.push(element[0]);
                        element.after("<div></div>");
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div><div my-directive></div><div my-directive></div></div>');
                var el1 = el[0].childNodes[0];
                var el2 = el[0].childNodes[1];
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenElements.length).toBe(2);
                expect(givenElements[0]).toBe(el1);
                expect(givenElements[1]).toBe(el2);
            });
        });

        //Linking Directives Across Multiple Nodes
        it("invokes multi-element directive link functions with whole group", () => {
            var givenElements;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    multiElement: true,
                    link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                        givenElements = element;
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive-start></div><p></p><div my-directive-end></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenElements.length).toBe(3);
            });
        });

        //Linking And Scope Inheritance
        it("makes new scope for element when directive asks for it", () => {
            var givenScope: IScope;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    scope: true,
                    link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                        givenScope = scope;
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenScope.$parent).toBe($rootScope);
            });
        });

        it("gives inherited scope to all directives on element", () => {
            var givenScope: IScope;
            var injector = makeInjectorWithDirectives({
                firstDirective() {
                    return {
                        scope: true,
                    };
                },
                secondDirective() {
                    return {
                        link(scope: IScope) {
                            givenScope = scope;
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div first-directive second-directive></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(givenScope.$parent).toBe($rootScope);
            });
        });

        it("adds scope class and data for element with the new scope", () => {
            var givenScope: IScope;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    scope: true,
                    link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                        givenScope = scope;
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $('<div my-directive></div>');
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(el.hasClass("ng-scope")).toBe(true);
                expect(el.data("$scope")).toBe(givenScope);
            });
        });

        //Isolate Scopes
    });
});