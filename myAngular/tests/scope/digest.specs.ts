import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import * as _ from 'lodash';
"use strict";

describe("scope", () => {
    describe("$digest", () => {
        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("can be constructed and used as an object", () => {
            scope.name = "1";
            expect(scope.name).toBe("1");
        });
        //Watching Object Properties: $watch And $digest
        it("calls the listener function of a watch on first $digest", () => {
            var watchFn = function () { return "wat"; };
            var listenerFn = jasmine.createSpy("wat");
            scope.$watch(watchFn, listenerFn);

            scope.$digest();

            expect(listenerFn).toHaveBeenCalled();
        });

        //Checking for Dirty Values
        it("calls the watch function with the scope as the argument", () => {
            var watchFn = jasmine.createSpy("wut");
            var listenerFn = function () { };
            scope.$watch(watchFn, listenerFn);

            scope.$digest();

            expect(watchFn).toHaveBeenCalledWith(scope);
        });

        it("calls the listener function when the watched value changes", () => {
            scope.name = "a";
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => { scope.counter++; }
            );

            expect(scope.counter).toBe(0);

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.name = "b";

            expect(scope.counter).toBe(1);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        //Initializing Watch Values
        it("calls listener when watch value is first undefined", () => {
            scope.counter = 0;
            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                });

            scope.$digest();
            expect(scope.counter).toBe(1);
        });

        it("calls listener with new value as old value the first time", () => {
            scope.name = "123";
            var oldValueGiven;

            scope.$watch(
                function (scope: IScopeEx) { return scope.name; },
                function (newValue, oldValue, scope) { oldValueGiven = oldValue; });

            scope.$digest();

            expect(oldValueGiven).toBe("123");
        });

        //Getting Notified Of Digests
        it("may have watchers that omit the listener function", () => {
            var watchFn = jasmine.createSpy("wit").and.returnValue("heahe-ho");
            scope.$watch(watchFn);

            scope.$digest();

            expect(watchFn).toHaveBeenCalled();
        });

        //Keeping The Digest Going While It Stays Dirty
        it("triggers chained watchers in the same digest", () => {
            scope.name = "Jane";

            scope.$watch(
                (scope: IScopeEx) => scope.nameUpper,
                (newValue: string, oldValue, scope: IScopeEx) => {
                    if (newValue) {
                        scope.initial = newValue.substring(0, 1) + ".";
                    }
                });

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue: string, oldValue, scope: IScopeEx) => {
                    if (newValue) {
                        scope.nameUpper = newValue.toUpperCase();
                    }
                });

            scope.$digest();
            expect(scope.initial).toBe("J.");

            scope.name = "Bob";
            scope.$digest();
            expect(scope.initial).toBe("B.");
        });

        it("gives up on the watches after 10 iterations", () => {
            scope.counter = 0;
            scope.counter2 = 0;

            scope.$watch((scope: IScopeEx) => scope.counter,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter2++;
                });

            scope.$watch((scope: IScopeEx) => scope.counter2,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                    if (scope.counter === 20) {
                        throw 'Failed test.';
                    }
                });

            expect(() => scope.$digest()).toThrow('10 digest iterations reached');
        }, 500);
        //Short-Circuiting The Digest When The Last Watch Is Clean
        it("ends the digest when the last watch is clean", () => {
            scope.array = _.range(100);
            var watchExecution = 0;

            _.times(100, i => {
                scope.$watch((scope: IScopeEx) => {
                    watchExecution++;
                    return scope.array[i];
                });
            });

            scope.$digest();
            expect(watchExecution).toBe(200);

            scope.array[0] = 420;
            scope.$digest();
            expect(watchExecution).toBe(301);
        });

        it("does not end digest so that new watches are not run", () => {
            scope.name = "abc";
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.$watch(
                        (scope: IScopeEx) => scope.name,
                        (newValue, oldValue, scope: IScopeEx) => {
                            scope.counter++;
                        });
                });

            scope.$digest();
            expect(scope.counter).toBe(1);
        });
        //Value-Based Dirty-Checking
        it("compares based on value if enabled", () => {
            scope.array = [1, 2, 3];
            scope.counter = 0;

            scope.$watch((scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                },
                true);

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.array.push(4);
            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("correctly handles NaNs", () => {
            scope.counter2 = 0 / 0;
            scope.counter = 0;

            scope.$watch((scope: IScopeEx) => scope.counter2,
                (newValue, oldValue, scope: IScopeEx) => { scope.counter++; });

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.$digest();
            expect(scope.counter).toBe(1);
        });

        it("catches exceptions in watch functions and continues", () => {
            scope.name = "abc";
            scope.counter = 0;

            scope.$watch(
                scope => { throw "Watcher Error"; },
            );

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => { scope.counter++; }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);
        });

        it("catches exceptions in listener functions and continues", () => {
            scope.name = "abc";
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope) => { throw "Listener Error"; }
            );

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => { scope.counter++; }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);
        });

        it("allows destroying a $watch with a removal function", () => {
            scope.name = "abc";
            scope.counter = 0;

            var destroyWatch = scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => { scope.counter++; }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.name = "def";
            scope.$digest();
            expect(scope.counter).toBe(2);


            scope.name = "ghi";
            destroyWatch();
            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("allows destroying a $watch during digest", () => {
            scope.name = "abc";

            var warchCalls: string[] = [];

            scope.$watch(
                (scope: IScopeEx) => {
                    warchCalls.push("first");
                    return scope.name;
                }
            );


            var destroyWatch = scope.$watch(
                scope => {
                    warchCalls.push("second");
                    destroyWatch();
                }
            );

            scope.$watch((scope: IScopeEx) => {
                warchCalls.push("third");
                return scope.name;
            });

            scope.$digest();
            expect(warchCalls).toEqual(["first", "second", "third", "first", "third"]);
        });

        it("allows a $watch to destroy another during digest", () => {
            scope.name = "abc";
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope) => {
                    destroyWatch();
                });

            var destroyWatch = scope.$watch(
                scope => { },
                (newValue, oldValue, scope) => { }
            );

            scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => { scope.counter++; }
            );

            scope.$digest();

            expect(scope.counter).toBe(1);
        });

        it("allows destroying several $watches during digest", () => {
            scope.name = "abc";
            scope.counter = 0;
            var logger = jasmine.createSpy("log", console.log);
            console.log = logger;

            var destroyWatch1 = scope.$watch(
                scope => {
                    destroyWatch1();
                    destroyWatch2();
                },
                (newValue, oldValue, scope) => { }
            );

            var destroyWatch2 = scope.$watch(
                (scope: IScopeEx) => scope.name,
                (newValue, oldValue, scope: IScopeEx) => { scope.counter++; }
            );

            scope.$digest();
            expect(scope.counter).toBe(0);
            expect(logger).toHaveBeenCalledTimes(0);
        });
    });
});