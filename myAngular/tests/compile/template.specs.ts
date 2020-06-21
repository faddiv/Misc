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

    //Basic Templating
    it("populates an element during compilation", () => {
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                template: "<div class='form-template'></div>"
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive></div>");
            $compile(el);

            expect(el.find("> .form-template").length).toBe(1);
        });
    });
    it("replaces any existing children", () => {
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                template: "<div class='form-template'></div>"
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive><div class='existing'></div></div>");
            $compile(el);

            expect(el.find("> .existing").length).toBe(0);
        });
    });

    it("compiles template contents also", () => {
        var compileSpy = jasmine.createSpy("compileSpy");
        var injector = makeInjectorWithDirectives({
            myDirective() {
                return {
                    template: "<div my-other-directive></div>"
                };
            },
            myOtherDirective() {
                return {
                    compile: compileSpy
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive></div>");
            $compile(el);

            expect(compileSpy).toHaveBeenCalled();
        });
    });
    //Disallowing More Than One Template Directive Per Element
    it("does not allow two directives with templates", () => {
        var injector = makeInjectorWithDirectives({
            myDirective() {
                return {
                    template: "<div></div>"
                };
            },
            myOtherDirective() {
                return {
                    template: "<div></div>"
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive my-other-directive></div>");
            expect(function () { $compile(el); }).toThrow();
        });
    });
    //Template Functions
    it("supports functions as template values", () => {
        var templateSpy = jasmine.createSpy("templateSpy")
            .and.returnValue("<div class='form-template'></div>");
        var injector = makeInjectorWithDirectives({
            myDirective() {
                return {
                    template: templateSpy
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive></div>");
            $compile(el);
            expect(el.find("> .form-template").length).toBe(1);
            var firstCall = templateSpy.calls.first();
            expect(firstCall.args[0][0]).toBe(el[0]);
            expect(firstCall.args[1].myDirective).toBeDefined();
        });
    });
    //Isolate Scope Directives with Templates
    it("uses isolate scope for template contents", () => {
        var linkSpy = jasmine.createSpy("linkSpy");
        var injector = makeInjectorWithDirectives({
            myDirective() {
                return {
                    scope: {
                        isoValue: "=myDirective"
                    },
                    template: "<div my-other-directive>"
                };
            },
            myOtherDirective() {
                return { link: linkSpy };
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-directive='42'></div>");
            $compile(el)($rootScope);
            var firstCall = linkSpy.calls.first();
            expect(firstCall.args[0]).not.toBe($rootScope);
            expect(firstCall.args[0].isoValue).toBe(42);
        });
    });
    //Asynchronous Templates: templateUrl
});
