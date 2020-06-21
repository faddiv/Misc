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

    it("allows changing an instance using a decorator", () => {
        module.factory("a", function () { return { aKey: 42 }; });
        module.decorator("a", function($delegate) {
            $delegate.decoratedKey = 43;
        });
        var injector = createInjector(["myModule"]);

        expect(injector.get<any>("a").aKey).toBe(42);
        expect(injector.get<any>("a").decoratedKey).toBe(43);
    });

    it("allows multiple decorators per service", () => {
        module.factory("a", function () { return { }; });
        module.decorator("a", function($delegate) {
            $delegate.decoratedKey = 42;
        });
        module.decorator("a", function($delegate) {
            $delegate.decoratedKey2 = 43;
        });
        var injector = createInjector(["myModule"]);

        expect(injector.get<any>("a").decoratedKey).toBe(42);
        expect(injector.get<any>("a").decoratedKey2).toBe(43);
    });
    
    it("uses dependency injection with decorators", () => {
        module.factory("a", function () { return {  }; });
        module.constant("b", 42)
        module.decorator("a", function(b, $delegate) {
            $delegate.decoratedKey = b;
        });
        var injector = createInjector(["myModule"]);

        expect(injector.get<any>("a").decoratedKey).toBe(42);
    });

});
