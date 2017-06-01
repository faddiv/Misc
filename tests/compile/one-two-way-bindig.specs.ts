"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes } from "angular";
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

    it("allows binding expression to isolate scope", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    anAttr: "<"
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
            expect(givenScope.anAttr).toEqual(42);
        });
    });
    
    it("allows aliasing expression attribute on isolate scope", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    anAttr: "<theAttr"
                },
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div my-directive the-attr="42"></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(givenScope.anAttr).toEqual(42);
        });
    });
    
    it("allows isolate scope expression on parent scope", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    myAttr: "<"
                },
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            $rootScope.parentAttr = 41;
            var el = $('<div my-directive my-attr="parentAttr + 1"></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect(givenScope.myAttr).toEqual(42);
        });
    });

    it("watches isolated scope expressions", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    myAttr: "<"
                },
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $('<div my-directive my-attr="parentAttr + 1"></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            $rootScope.parentAttr = 41;
            $rootScope.$digest();
            expect(givenScope.myAttr).toEqual(42);
        });
    });
    
    it("does not watch optional missing isolate scope expressions", () => {
        var givenScope: IScope;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                scope: {
                    myAttr: "<?"
                },
                link(scope: IScope, element: JQuery[], attrs: IAttributes) {
                    givenScope = scope;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScopeEx) {
            var el = $('<div my-directive></div>');
            var linkFn = $compile(el);
            linkFn($rootScope);
            expect($rootScope.$$watchers.length).toEqual(0);
        });
    });
    //798 - missing test for $destroy
});