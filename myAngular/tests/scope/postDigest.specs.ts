import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("$postDigest", () => {

        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("runs after each digest", () => {
            scope.counter = 0;

            scope.$$postDigest(() => {
                scope.counter++;
            });

            expect(scope.counter).toBe(0);
            scope.$digest();

            expect(scope.counter).toBe(1);
            scope.$digest();

            expect(scope.counter).toBe(1);
        });

        it("does not include $$postDigest in the digest", () => {
            scope.text = "original value";

            scope.$$postDigest(() => {
                scope.text = "changed value";
            });

            scope.$watch(
                (scope: IScopeEx) => scope.text,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.watchedWalue = newValue;
                }
            );

            scope.$digest();
            expect(scope.watchedWalue).toBe("original value");

            scope.$digest();
            expect(scope.watchedWalue).toBe("changed value");
        });


        it("catches exceptions in $$postDigest", () => {
            var didRun = false;;

            scope.$$postDigest(() => {
                throw "postDigest error";
            });

            scope.$$postDigest(() => {
                didRun = true;
            });


            scope.$digest();
            expect(didRun).toBe(true);
        });
    });
});