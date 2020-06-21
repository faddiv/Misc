import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    var scope: IScopeEx;

    beforeEach(() => {
    });

    it("allows configuring a shorter ttl", () => {
        var injector = createInjector(["ng", function ($rootScopeProvider: IRootScopeProvider) {
            $rootScopeProvider.digestTtl(5);
        }]);
        var scope = <IScopeEx>injector.get("$rootScope");
        scope.counter = 0;
        scope.counter2 = 0;

        scope.$watch(
            function (scope: IScopeEx) { return scope.counter; },
            function () {
                if (scope.counter2 < 5) {
                    scope.counter2++;
                }
            }
        );

        scope.$watch(
            function (scope: IScopeEx) { return scope.counter2; },
            function () {
                scope.counter++;
            }
        )
        expect(function () {
            scope.$digest();
        }).toThrow();
    });
});