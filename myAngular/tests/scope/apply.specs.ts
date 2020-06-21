import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("$apply", () => {

        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("executes the given function and starts the digest", () => {
            scope.text = "someValue";
            scope.counter = 0;

            scope.$watch(
                (scope: IScopeEx) => scope.text,
                (newValue, oldValue, scope: IScopeEx) => {
                    scope.counter++;
                }
            );

            scope.$digest();
            expect(scope.counter).toBe(1);

            scope.$apply((scope: IScopeEx) => {
                scope.text = "otherValue"
            });
            expect(scope.counter).toBe(2);
        });

        //TODO: I think there should be two more test, that test these features:
        //Do $digest() even when exception thrown.
        //Returns the function return value.
    });
});