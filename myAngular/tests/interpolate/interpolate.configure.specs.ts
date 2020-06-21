import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import {
    auto, IInterpolateProvider, IInterpolateService, ICompileProvider, ICompileService, IScope
} from "angular";
import { publishExternalAPI } from "../../src/angular_public";
import { IInterpolationFunctionInternal } from "../../src/angularInterfaces";

describe("$interpolate", () => {
    beforeEach(() => {
        publishExternalAPI();
    });

    afterEach(() => {
        delete window.angular;
    });

    it("allows configuring start and end symbols", () => {

        var injector = createInjector(["ng", function ($interpolateProvider: IInterpolateProvider) {
            $interpolateProvider.startSymbol("FOO").endSymbol("OOF");
        }]);
        var $interpolate = injector.get("$interpolate");
        expect($interpolate.startSymbol()).toEqual("FOO");
        expect($interpolate.endSymbol()).toEqual("OOF");
    });

    it("allows configuring start and end symbols", () => {

        var injector = createInjector(["ng", function ($interpolateProvider: IInterpolateProvider) {
            $interpolateProvider.startSymbol("FOO").endSymbol("OOF");
        }]);
        var $interpolate = injector.get("$interpolate");
        var interp = $interpolate("FOOexprOOF");
        expect(interp({ expr: 42 })).toEqual("42");
    });

    it("does not work with default symbols when reconfigured", () => {

        var injector = createInjector(["ng", function ($interpolateProvider: IInterpolateProvider) {
            $interpolateProvider.startSymbol("FOO").endSymbol("OOF");
        }]);
        var $interpolate = injector.get("$interpolate");
        var interp = $interpolate("{{expr}}");
        expect(interp({ expr: 42 })).toEqual("{{expr}}");
    });

    it("supports unescaping for reconfigured symbols", () => {

        var injector = createInjector(["ng", function ($interpolateProvider: IInterpolateProvider) {
            $interpolateProvider.startSymbol("FOO").endSymbol("OOF");
        }]);
        var $interpolate = injector.get("$interpolate");
        var interp = $interpolate("\\F\\O\\Oexpr\\O\\O\\F");
        expect(interp({ expr: 42 })).toEqual("FOOexprOOF");
    });

    it("denormailzes directive template", () => {
        var injector = createInjector(["ng", function ($interpolateProvider: IInterpolateProvider, $compileProvider: ICompileProvider) {
            $interpolateProvider.startSymbol("[[").endSymbol("]]");
            $compileProvider.directive("myDirective", function () {
                return {
                    template: "Value is {{expr}}"
                };
            });
        }]);
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-directive></div>");
            $rootScope.expr = 42;
            $compile(el)($rootScope);
            $rootScope.$apply();

            expect(el.html()).toEqual("Value is 42");
        });
    });
});
