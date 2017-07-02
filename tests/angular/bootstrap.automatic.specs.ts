"use strict";
import { auto, IScope } from "angular";
import { default as readyFn } from "../../src/bootstrap";

describe("bootstrap", () => {
    var injector: auto.IInjectorService;

    describe("auto", () => {
        var element: JQuery;
        var body: JQuery;
        beforeAll(function () {
        });
        beforeEach(function () {
            body = $("body");
        });
        afterEach(function () {
            element.remove();
            //delete window.angular;
        });

        ["ng-", "data-ng-", "ng:", "x-ng-"].forEach(prefix => {
            it("bootstraps if there is " + prefix + "app in the dom", () => {
                element = $("<div " + prefix + "app='myModule'><div>");
                element.appendTo(body);
                window.angular.module("myModule", []);
                readyFn();

                expect(element.data("$injector")).toBeDefined();
            });

            it("apply " + prefix + "strict-di attribute", () => {
                element = $("<div ng-app='myModule' " + prefix + "strict-di><div my-directive></div><div>");
                element.appendTo(body);
                window.angular.module("myModule", [])
                    .constant("aValue", 42)
                    .directive("myDirective", function (aValue) {
                        return {};
                    });

                expect(function () {
                    readyFn();
                }).toThrow();
            });
        });
    });
});