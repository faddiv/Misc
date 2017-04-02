import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    var scope: IScopeEx;

    beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
    });

    it("removes constant watches after first invocation", () => {
        scope.$watch("[1,2,3]", () => { });
        scope.$digest();

        expect(scope.$$watchers.length).toBe(0);
    });

    it("accepts one-time watches", () => {
        var theValue;

        scope.number = 42
        scope.$watch("::number", function (newValue, oldValue, scope) {
            theValue = newValue;
        });
        scope.$digest();

        expect(theValue).toBe(42);
    });

    it("removes one-time watches after first invocation", () => {
        scope.number = 42;
        scope.$watch("::number", () => { });
        scope.$digest();

        expect(scope.$$watchers.length).toBe(0);
    });

    it("does not remove one-time-watches until value is defined", () => {
        scope.$watch("::number", () => { });
        scope.$digest();
        expect(scope.$$watchers.length).toBe(1);
        scope.number = 42;
        scope.$digest();
        expect(scope.$$watchers.length).toBe(0);
    });

    it("does not remove one-time-watches until value stays defined", () => {
        scope.number2 = 42;

        scope.$watch("::number2", () => { });
        var unwatchDeleter = scope.$watch("number2", () => {
            delete scope.number2;
        });

        scope.$digest();
        expect(scope.$$watchers.length).toBe(2);

        scope.number2 = 42;
        unwatchDeleter();
        scope.$digest();
        expect(scope.$$watchers.length).toBe(0);
    });

    it("does not remove one-time watches before all array items defined", () => {
        scope.$watch("::[1,2,number]", function () { }, true);

        scope.$digest();
        expect(scope.$$watchers.length).toBe(1);

        scope.number = 3;
        scope.$digest();
        expect(scope.$$watchers.length).toBe(0);
    });

    it("does not remove one-time watches before all object vals defined", () => {
        scope.$watch("::{a:1,b:2,c:number}", function () { }, true);

        scope.$digest();
        expect(scope.$$watchers.length).toBe(1);

        scope.number = 3;
        scope.$digest();
        expect(scope.$$watchers.length).toBe(0);
    });

    it("does not re-evaluate an array if its contents do not change", () => {
        var values = [];
        scope.number = 1;
        scope.number2 = 2;
        scope.counter = 3;

        scope.$watch("[number, number2, counter]", function (value) {
            values.push(value);
        });

        scope.$digest();
        expect(values.length).toBe(1);
        expect(values[0]).toEqual([1, 2, 3]);

        scope.$digest();
        expect(values.length).toBe(1);

        scope.counter = 4;
        scope.$digest();
        expect(values.length).toBe(2);
        expect(values[1]).toEqual([1, 2, 4]);
    });
});
