"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IControllerService, auto, IControllerProvider, ICompileProvider, ICompileService, IScope, IAttributes } from "angular";
import $ from "jquery";

describe("ngTransclude", () => {
    beforeEach(function () {
        publishExternalAPI();
    });

    afterEach(() => {
        delete window.angular;
    });

    function makeInjectorWithTranscluderTemplate(template) {
        return createInjector(["ng", function ($compileProvider: ICompileProvider) {
            $compileProvider.directive({
                myTranscluder() {
                    return {
                        transclude: true,
                        template: template
                    };
                }
            });
        }]);
    }

    //The ngTransclude Directive
    it("transcludes the parent directive transclusion", () => {
        var injector = makeInjectorWithTranscluderTemplate(
            "<div ng-transclude></div>"
        );
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-transcluder>Hello</div>");
            $compile(el)($rootScope);
            expect(el.find("> [ng-transclude]").html()).toEqual("Hello");
        });
    });

    it("empties existing contents", () => {
        var injector = makeInjectorWithTranscluderTemplate(
            "<div ng-transclude>Existing contents</div>"
        );
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-transcluder>Hello</div>");
            $compile(el)($rootScope);
            expect(el.find("> [ng-transclude]").html()).toEqual("Hello");
        });
    });

    it("my used as element", () => {
        var injector = makeInjectorWithTranscluderTemplate(
            "<ng-transclude>Existing contents</ng-transclude>"
        );
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-transcluder>Hello</div>");
            $compile(el)($rootScope);
            expect(el.find("> ng-transclude").html()).toEqual("Hello");
        });
    });

    it("may be used as class", () => {
        var injector = makeInjectorWithTranscluderTemplate(
            "<div class='ng-transclude'>Existing contents</div>"
        );
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-transcluder>Hello</div>");
            $compile(el)($rootScope);
            expect(el.find("> .ng-transclude").html()).toEqual("Hello");
        });
    });
});