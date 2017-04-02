import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("scope", () => {
    var scope: IScopeEx;
    var parse: IParseService

    beforeEach(() => {
            publishExternalAPI();
            var injector = createInjector(["ng"]);
            scope = injector.get("$rootScope");
            parse = injector.get("$parse");
    });

    it("allows calling assign on identifier expressions", () => {
        var fn = parse("number");
        expect(fn.assign).toBeDefined();
        var scope = <IScopeEx>{};
        fn.assign(scope, 42);
        expect(scope.number).toBe(42);
    });

    it("allows calling assign on member expressions", () => {
        var fn = parse("obj.number");
        expect(fn.assign).toBeDefined();
        var scope = <IScopeEx>{};
        fn.assign(scope, 42);
        expect(scope.obj).toEqual({ number: 42 });
    });
});