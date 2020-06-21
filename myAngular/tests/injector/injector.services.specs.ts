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

    it("allows registering a service", () => {
        class MyService {
            getValue() {
                return 42;
            }
        }
        module.service("a", MyService);

        var injector = createInjector(["myModule"]);

        expect(injector.get<MyService>("a").getValue()).toBe(42);
    });

    it("injects service constructors with instances", () => {
        class MyService {
            constructor(private value: number) {

            }
            getValue() {
                return this.value;
            }
        }
        module.value("value", 42);
        module.service("a", MyService);

        var injector = createInjector(["myModule"]);

        expect(injector.get<MyService>("a").getValue()).toBe(42);
    });

    it("only calls a service function once", () => {
        module.service("a", function MyService() { });
        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(injector.get("a"));
    });

});
