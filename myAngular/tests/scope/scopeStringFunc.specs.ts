import * as _ from 'lodash';
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

    it("accepts expressions for watch values", () => {
        var theValue;
        scope.number = 42;
        scope.$watch("number", function (newValue, oldValue, scope) {
            theValue = newValue;
        });
        scope.$digest();

        expect(theValue).toBe(42);
    });

    it("accepts expressions for watchCollection function", () => {
        var theValue;
        scope.array = [1, 2, 3];
        scope.$watchCollection("array", function (newValue, oldValue, scope) {
            theValue = newValue;
        });
        scope.$digest();

        expect(theValue).toEqual([1, 2, 3]);
    });

    it("accepts expressions in $eval", () => {
        expect(scope.$eval("42")).toBe(42);
    });

    it("accepts expressions in $apply", () => {
        scope.aFunction = _.constant(42);
        expect(scope.$apply("aFunction()")).toBe(42);
    });

    it("accepts expressions in $evalAsync", (done) => {
        var called = false;
        scope.aFunction = function () { called = true; };
        scope.$evalAsync("aFunction()");
        scope.$$postDigest(function()  {
            expect(called).toBe(true);
            done();
        });
    });
    //additional test for $applyAsync
    it("accepts expressions in $applyAsync", (done) => {
        var called = false;
        scope.aFunction = function () { called = true; };
        scope.$applyAsync("aFunction()");
        scope.$$postDigest(function()  {
            expect(called).toBe(true);
            done();
        });
    });
    //additional test for $watchGroup
    it("accepts expressions in $watchGroup", () => {
        var called = false;
        scope.aFunction = function () { called = true; };
        scope.$watchGroup(["aFunction()"], () => {
            called = true;
        });
        scope.$digest();
        expect(called).toBe(true);
    });
    
});