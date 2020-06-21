import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("$applyAsync", () => {

        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("allows async $apply with $applyAsync", done => {
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.text,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                });

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.$applyAsync((scope: IScopeEx) => {
                scope.text = "adsf";
            });
            expect(scope.counter).toBe(1);

            setTimeout(() => {
                expect(scope.counter).toBe(2);
                done();
            }, 50);
        });

        it("never executes $applyAsynced function in the same cycle", done => {
            scope.array = [1, 2, 3];
            scope.asyncApplied = false;

            scope.$watch(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope) => {
                    scope.$applyAsync((scope: IScopeEx) => {
                        scope.asyncApplied = true;
                    });
                });

            scope.$digest();
            expect(scope.asyncApplied).toBe(false);
            setTimeout(() => {
                expect(scope.asyncApplied).toBe(true);
                done();
            }, 50);
        });

        it("coalesces many calls to $applyAsync", done => {
            scope.counter = 0;

            scope.$watch((scope: IScopeEx) => {
                scope.counter++;
                return scope.text;
            });

            scope.$applyAsync((scope: IScopeEx) => {
                scope.text = "abc";
            });

            scope.$applyAsync((scope: IScopeEx) => {
                scope.text = "def";
            });

            setTimeout(() => {
                expect(scope.counter).toBe(2);
                done();
            }, 50);
        });

        it("cancels and flushes $applyAsync if digested first", done => {
            scope.counter = 0;

            scope.$watch((scope: IScopeEx) => {
                scope.counter++;
                return scope.text;
            });

            scope.$applyAsync((scope: IScopeEx) => {
                scope.text = "abc";
            });

            scope.$applyAsync((scope: IScopeEx) => {
                scope.text = "def";
            });

            scope.$digest();
            expect(scope.counter).toBe(2);
            expect(scope.text).toEqual("def");

            setTimeout(() => {
                expect(scope.counter).toBe(2);
                done();
            }, 50);
        });

        it("catches exceptions in $applyAsync", done => {
            scope.asyncApplied = false;

            scope.$applyAsync((scope: IScopeEx) => {
                throw "applyAsync error";
            });

            scope.$applyAsync((scope: IScopeEx) => {
                throw "applyAsync error";
            });

            scope.$applyAsync((scope: IScopeEx) => {
                scope.asyncApplied = true;
            });

            setTimeout(() => {
                expect(scope.asyncApplied).toBe(true);
                done();
            }, 50);
        });
    });
});