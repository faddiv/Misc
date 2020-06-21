import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("$evalAsync", () => {

        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("executes given function later in the same cycle", () => {
            scope.array = [1, 2, 3];
            scope.asyncEvaluated = false;
            scope.asyncEvaluatedImmediatelly = false;

            scope.$watch(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.$evalAsync((scope: IScopeEx) => {
                        scope.asyncEvaluated = true;
                    });
                    scope.asyncEvaluatedImmediatelly = scope.asyncEvaluated;
                });

            scope.$digest();
            expect(scope.asyncEvaluated).toBe(true);
            expect(scope.asyncEvaluatedImmediatelly).toBe(false);
        });

        it("executes $evalAsynced functions added by watch functions", () => {
            scope.array = [1, 2, 3];
            scope.asyncEvaluated = false;

            scope.$watch(
                (scope: IScopeEx) => {
                    if (!scope.asyncEvaluated) {
                        scope.$evalAsync((scope: IScopeEx) => {
                            scope.asyncEvaluated = true;
                        });
                    }
                    return scope.array;
                });

            scope.$digest();
            expect(scope.asyncEvaluated).toBe(true);
        });

        it("executes $evalAsynced functions even when not dirty", () => {
            scope.array = [1, 2, 3];
            scope.asyncEvaluatedTimes = 0;

            scope.$watch(
                (scope: IScopeEx) => {
                    if (scope.asyncEvaluatedTimes < 2) {
                        scope.$evalAsync((scope: IScopeEx) => {
                            scope.asyncEvaluatedTimes++;
                        });
                    }
                    return scope.array;
                });

            scope.$digest();
            expect(scope.asyncEvaluatedTimes).toBe(2);
        });

        it("eventually halts $evalAsyncs added by watches", () => {
            scope.array = [1, 2, 3];
            scope.asyncEvaluatedTimes = 0;

            scope.$watch(
                (scope: IScopeEx) => {
                    if (scope.asyncEvaluatedTimes < 100) {
                        scope.$evalAsync((scope: IScopeEx) => {
                            scope.asyncEvaluatedTimes++;
                        });
                    }
                    return scope.array;
                });


            expect(() => {
                scope.$digest();
            }).toThrow();
        });

        it("schedules a digest in $evalAsync", (done: Function) => {
            scope.text = "abc";
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.text,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$evalAsync(() => { });

            expect(scope.counter).toBe(0);
            setTimeout(function () {
                expect(scope.counter).toBe(1);
                done();
            }, 50);
        });

        it("catches exceptions in $evalAsync", done => {
            scope.text = "abc";
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.text,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$evalAsync(scope => {
                throw "evalAsync Error";
            });

            setTimeout(function () {
                expect(scope.counter).toBe(1);
                done();
            }, 50);
        });
    });
});