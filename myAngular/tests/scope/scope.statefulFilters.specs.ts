import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    
    beforeEach(() => {
        publishExternalAPI();
    });

    it("allows $stateful filter value to change over time", () => {
        var counter = 0;
        var injector = createInjector(["ng", function ($filterProvider: IFilterProvider) {
            $filterProvider.register("withTime", function () {
                var fun = <IFilter>function (v: any) {
                    return counter + v;
                };
                fun.$stateful = true;
                return fun;
            });
        }]);
        var scope = injector.get("$rootScope");

        var listenerSpy = jasmine.createSpy("listener");
        scope.$watch("42|withTime", listenerSpy);
        scope.$digest();
        var firstValue = listenerSpy.calls.mostRecent().args[0];
        counter++;
        scope.$digest();
        var secondValue = listenerSpy.calls.mostRecent().args[0];
        expect(secondValue).not.toEqual(firstValue);
    });
});