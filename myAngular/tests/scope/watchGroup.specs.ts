import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("$watchGroup", () => {

        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("takes watches as an array and calls listener with arrays", () => {
            var gotNewValues: number[];
            var gotOldValues: number[];

            scope.number = 1;
            scope.number2 = 2;

            scope.$watchGroup([
                (scope: IScopeEx) => scope.number,
                (scope: IScopeEx) => scope.number2],
                (newValues, oldValues, scope) => {
                    gotNewValues = newValues;
                    gotOldValues = oldValues;
                });
            scope.$digest();

            expect(gotNewValues).toEqual([1, 2]);
            expect(gotOldValues).toEqual([1, 2]);
        });

        it("only calls listener once per digest", () => {
            var counter = 0;

            scope.number = 1;
            scope.number2 = 2;

            scope.$watchGroup([
                (scope: IScopeEx) => scope.number,
                (scope: IScopeEx) => scope.number2],
                (newValues, oldValues, scope) => {
                    counter++;
                });
            scope.$digest();

            expect(counter).toEqual(1);
        });

        it("uses the same array of old and new values on first run", () => {
            var gotNewValues: number[];
            var gotOldValues: number[];

            scope.number = 1;
            scope.number2 = 2;

            scope.$watchGroup([
                (scope: IScopeEx) => scope.number,
                (scope: IScopeEx) => scope.number2],
                (newValues, oldValues, scope) => {
                    gotNewValues = newValues;
                    gotOldValues = oldValues;
                });
            scope.$digest();

            expect(gotNewValues).toBe(gotOldValues);
        });

        it("uses different arrays for old and new values on subsequent runs", () => {
            var gotNewValues: number[];
            var gotOldValues: number[];

            scope.number = 1;
            scope.number2 = 2;

            scope.$watchGroup([
                (scope: IScopeEx) => scope.number,
                (scope: IScopeEx) => scope.number2],
                (newValues, oldValues, scope) => {
                    gotNewValues = newValues;
                    gotOldValues = oldValues;
                });
            scope.$digest();

            scope.number2 = 3;
            scope.$digest();

            expect(gotNewValues).toEqual([1, 3]);
            expect(gotOldValues).toEqual([1, 2]);
        });

        it("calls the listener once when the watch array is empty", () => {
            var gotNewValues: number[];
            var gotOldValues: number[];

            scope.$watchGroup([],
                (newValues, oldValues, scope) => {
                    gotNewValues = newValues;
                    gotOldValues = oldValues;
                });
            scope.$digest();

            expect(gotNewValues).toEqual([]);
            expect(gotOldValues).toEqual([]);
        });

        it("can be deregistered", () => {
            var counter = 0;

            scope.number = 1;
            scope.number2 = 2;

            var destroyGroup = scope.$watchGroup([
                (scope: IScopeEx) => scope.number,
                (scope: IScopeEx) => scope.number2
            ], () => {
                counter++;
            });
            scope.$digest();

            scope.number2 = 3;
            destroyGroup();
            scope.$digest();

            expect(counter).toBe(1);
        });

        it("does not call the zero-watch listener when deregistered first", () => {
            var counter = 0;

            var destroyGroup = scope.$watchGroup([], () => {
                counter++;
            });
            destroyGroup();
            scope.$digest();

            expect(counter).toBe(0);
        });
    });
});