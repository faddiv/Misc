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
});