import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IFilterProvider, auto } from "angular";
"use strict";

describe("filter", () => {

    beforeEach(() => {
        publishExternalAPI();
    });

    it("can be registered and obtained", () => {
        var myFilter = function () { };
        var myFilterFactory = function () {
            return myFilter;
        };
        var injector = createInjector(["ng", function ($filterProvider: IFilterProvider) {
            $filterProvider.register("my", myFilterFactory);
        }]);
        var $filter = injector.get("$filter");
        expect($filter("my")).toBe(myFilter);
    });

    it("allows registering multiple filters with an object", () => {
        var myFilter = function () { };
        var myOtherFilter = function () { };

        var injector = createInjector(["ng", function ($filterProvider: IFilterProvider) {
            $filterProvider.register({
                my: function () {
                    return myFilter;
                },
                myOther: function () {
                    return myOtherFilter;
                }
            });
        }]);
        var $filter = injector.get("$filter");

        expect($filter("my")).toBe(myFilter);
        expect($filter("myOther")).toBe(myOtherFilter);
    });

    it("is available through injection", () => {
        var myFilter = function () { };
        var myFilterFactory = function () {
            return myFilter;
        };
        var injector = createInjector(["ng", function ($filterProvider: IFilterProvider) {
            $filterProvider.register("my", myFilterFactory);
        }]);
        expect(injector.has("myFilter")).toBe(true);
        expect(injector.get("myFilter")).toBeDefined();
    });

    it("may have dependencies in factory", () => {
        var myFilter = function () { };
        var injector = createInjector(["ng", function ($provide: auto.IProvideService, $filterProvider: IFilterProvider) {
            $provide.constant("suffix", "!");
            $filterProvider.register("my", function (suffix) {
                return function (v) {
                    return v + suffix;
                };
            });
        }]);
        var filter = injector.get<Function>("myFilter");
        expect(filter("42")).toEqual("42!");
    });

    it("can be registered through module API", () => {
        var myFilter = function () { };
        var module = window.angular.module("myModule", []);
        module.filter("my", function () {
            return myFilter;
        });
        var injector = createInjector(["ng", "myModule"]);
        expect(injector.has("myFilter")).toBe(true);
        expect(injector.get("myFilter")).toBeDefined();
    });
});