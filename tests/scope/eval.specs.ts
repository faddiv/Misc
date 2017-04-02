import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    describe("$eval", () => {

        var scope: IScopeEx;

        beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
        });

        it("executes $evaled function and returns result", () => {
            scope.text = "42";

            var result = scope.$eval((scope: IScopeEx) => { return scope.text; });

            expect(result).toBe("42");
        });

        it("passes the second $eval argument straight through", () => {
            scope.number = 42;

            var result = scope.$eval((scope: IScopeEx, arg) => { return scope.number + arg; }, 2);

            expect(result).toBe(44);
        });

        it('has a $$phase field whose value is the current digest phase', () => {
            //TODO: not implemented, since this test is a "The Inspector" antipattern.
        });
    });
});