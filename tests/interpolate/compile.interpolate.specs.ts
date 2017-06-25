"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope } from "angular";
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
    });
});
