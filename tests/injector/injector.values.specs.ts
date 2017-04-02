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

    it("allows registering a value", () => {
        module.value("a", 42);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("does not make values available to config blocks", () => {
        module.value("a", 42);
        module.config(function (a) { })
        expect(function () {
            createInjector(["myModule"]);
        }).toThrow();

    });
    it("allows an undefined value", () => {
        module.value("a", undefined);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBeUndefined();
    });
});
