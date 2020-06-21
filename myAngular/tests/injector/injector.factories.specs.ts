import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IModule } from "angular";

describe("setupModuleLoader", () => {
    var ng: IAngularStatic;
    var module: IModule;
    beforeEach(() => {
        delete window.angular;
        setupModuleLoader(window);
        ng = window.angular;
        module = ng.module("myModule", []);
    });

    afterEach(() => {
        delete window.angular;
    });

    it("allows registering a factory", () => {
        module.factory("a", function () { return 42; });

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("injects a factory function with instances", () => {
        module.factory("a", function () { return 40; });
        module.factory("b", function (a) { return a + 2; });
        var injector = createInjector(["myModule"]);
        expect(injector.get("b")).toBe(42);
    });

    it("only calls a factory function once", () => {
        module.factory("a", function () { return {}; });
        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(injector.get("a"));
    });

    it("forces a factory to return a value", () => {
        module.factory("a", function () { });
        module.factory("b", function () { return null; });
        var injector = createInjector(["myModule"]);
        expect(function () {
            injector.get("a");
        }).toThrow();
        expect(injector.get("b")).toBeNull();
    });
});
