import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("$watchCollection", () => {

        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("works like a normal $watch for non-collections", () => {
            var valueProvided;

            scope.number = 42;
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.number,
                (newValue, oldValue, scope: IScopeEx) => {
                    valueProvided = newValue;
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);
            expect(valueProvided).toBe(scope.number);

            scope.number = 45;
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();

            expect(scope.counter).toBe(2);
        });

        it("works like a normal watch for NaNs", () => {
            scope.number = 0 / 0;
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.number,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.$digest();
            expect(scope.counter).toBe(1);
        });

        it("notices when the value becomes an array", () => {
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.array = [1, 2, 3];
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices an item added to an array", () => {
            scope.array = [1, 2, 3];
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.array.push(4);
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices an item removed from an array", () => {
            scope.array = [1, 2, 3];
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.array.shift();
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices an item replaced in an array", () => {
            scope.array = [1, 2, 3];
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.array[1] = 42;
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices items reordered in an array", () => {
            scope.array = [2, 1, 3];
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.array.sort();
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("does not fail on NaNs in arrays", () => {
            scope.array = [2, NaN, 3];
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);
        });

        it("notices an item replaced in an arguments object", () => {
            (function (a, b, c) {
                scope.args = arguments;
            })(1, 2, 3);
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.args,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.args[1] = 42;
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices an item replaced in an arguments object", () => {
            document.documentElement.appendChild(document.createElement("div"));
            scope.nodeList = document.getElementsByTagName("div");
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.nodeList,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            document.documentElement.appendChild(document.createElement("div"));
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices when the value becomes an object", () => {
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.obj = { a: 1 };
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices when an attribute is added to an object", () => {
            scope.counter = 0;
            scope.obj = { a: 1 };

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.obj.b = 2;
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("notices when an attribute is changed in an object", () => {
            scope.counter = 0;
            scope.obj = { a: 1 };

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.obj.a = 2;
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("does not fail on NaN attributes in objects", () => {
            scope.counter = 0;
            scope.obj = { a: NaN };

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);
        });

        it("notices when an attribute is removed from an object", () => {
            scope.counter = 0;
            scope.obj = { a: 1, b: 1 };

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            delete scope.obj.a;
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.$digest();
            expect(scope.counter).toBe(2);
        });

        it("does not consider any object with a length property an array", () => {
            scope.obj = { length: 42, otherKey: "asdf" };
            scope.counter = 0;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.obj.newKey = "fsad";
            scope.$digest();
            expect(scope.counter).toBe(2);

            scope.obj.newKey = "plss";
            scope.$digest();
            expect(scope.counter).toBe(3);
        });

        it("gives the old non-collection value to listeners", () => {
            scope.number = 42;
            var oldValueGiven;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.number,
                (newValue, oldValue, scope: IScopeEx) => {
                    oldValueGiven = oldValue;
                }
            );

            scope.$digest();

            scope.number = 43;
            scope.$digest();

            expect(oldValueGiven).toBe(42);
        });

        it("gives the old array value to listeners", () => {
            scope.array = [1, 2, 3];
            var oldValueGiven;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.array,
                (newValue, oldValue, scope: IScopeEx) => {
                    oldValueGiven = oldValue;
                }
            );

            scope.$digest();

            scope.array.push(42);
            scope.$digest();

            expect(oldValueGiven).toEqual([1, 2, 3]);
        });

        it("gives the old object value to listeners", () => {
            scope.obj = { a: 1, b: 2 };
            var oldValueGiven;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    oldValueGiven = oldValue;
                }
            );

            scope.$digest();

            scope.obj.c = 3;
            scope.$digest();

            expect(oldValueGiven).toEqual({ a: 1, b: 2 });
        });

        it("uses the new value as the old value on first digest", () => {
            scope.obj = { a: 1, b: 2 };
            var oldValueGiven;

            scope.$watchCollection(
                (scope: IScopeEx) => scope.obj,
                (newValue, oldValue, scope: IScopeEx) => {
                    oldValueGiven = oldValue;
                }
            );

            scope.$digest();

            expect(oldValueGiven).toEqual({ a: 1, b: 2 });
        });
    });
});