"use strict";
import { auto, IScope } from "angular";
import "../../src/bootstrap";

describe("bootstrap", () => {
    var injector: auto.IInjectorService;

    describe("manual", () => {
        it("is available", () => {
            expect(window.angular.bootstrap).toBeDefined();
        });

        it("creates and returns an injector", () => {
            var element = $("<div></div>");
            var injector = window.angular.bootstrap(element);
            expect(injector).toBeDefined();
            expect(injector.invoke).toBeDefined();
        });

        it("attaches the injector to the bootstrapped element", () => {
            var element = $("<div></div>");
            var injector = window.angular.bootstrap(element);
            expect(element.data("$injector")).toBe(injector);
        });

        it("loads built-ins into the injector", () => {
            var element = $("<div></div>");
            var injector = window.angular.bootstrap(element);
            expect(injector.has("$compile")).toBe(true);
            expect(injector.has("$rootScope")).toBe(true);
        });

        it("loads other specified modules into the injector", () => {
            var element = $("<div></div>");

            window.angular.module("myModule", [])
                .constant("aValue", 42);
            window.angular.module("myOtherModule", [])
                .constant("aSecondValue", 43);
            var injector = window.angular.bootstrap(element, ["myModule", "myOtherModule"]);
            expect(injector.get("aValue")).toBe(42);
            expect(injector.get("aSecondValue")).toBe(43);
        });

        it("makes root element available for injection", () => {
            var element = $("<div></div>");

            var injector = window.angular.bootstrap(element);
            expect(injector.has("$rootElement")).toBe(true);
            expect(injector.get("$rootElement")[0]).toBe(element[0]);
        });

        it("compiles the element", () => {
            var compileSpy = jasmine.createSpy("compileSpy");
            var element = $("<div><div my-directive></div></div>");
            window.angular.module("myModule", [])
                .directive("myDirective", function () {
                    return { compile: compileSpy }
                });
            var injector = window.angular.bootstrap(element, ["myModule"]);
            expect(compileSpy).toHaveBeenCalled();
        });

        it("links the element", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var element = $("<div><div my-directive></div></div>");
            window.angular.module("myModule", [])
                .directive("myDirective", function () {
                    return { link: linkSpy }
                });
            var injector = window.angular.bootstrap(element, ["myModule"]);
            expect(linkSpy).toHaveBeenCalled();
            expect(linkSpy.calls.mostRecent().args[0]).toEqual(
                injector.get("$rootScope")
            );
        });

        it("runs a digest", () => {
            var element = $("<div><div my-directive>{{expr}}</div></div>");
            window.angular.module("myModule", [])
                .directive("myDirective", function () {
                    return {
                        link: function (scope: IScope) {
                            scope.expr = 42;
                        }
                    };
                });
            var injector = window.angular.bootstrap(element, ["myModule"]);
            expect(element.find("[my-directive]").text()).toBe("42");
        });

        it("supports enabling strictDi mode", () => {
            var element = $("<div><div my-directive></div></div>");
            window.angular.module("myModule", [])
                .constant("aValue", 42)
                .directive("myDirective", function (aValue) {
                    return {};
                });

            expect(function () {
                window.angular.bootstrap(element, ["myModule"], { strictDi: true });
            }).toThrow();
        });
    });
});