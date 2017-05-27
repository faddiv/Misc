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
    });
});