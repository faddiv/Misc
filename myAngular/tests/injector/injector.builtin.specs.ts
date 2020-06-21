import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IModule, auto, IServiceProvider, IServiceProviderClass } from "angular";

describe("injector", () => {
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

    it("allows injecting the instance injector to $get", () => {
        class AProvider implements IServiceProvider {
            constructor() {

            }
            $get($injector: auto.IInjectorService) {
                return $injector.get("a");
            }
        }
        module.constant("a", 42);
        module.provider("b", AProvider);

        var injector = createInjector(["myModule"]);

        expect(injector.get("b")).toBe(42);
    });

    it("allows injecting the provider injector to provider", () => {
        class AProvider implements IServiceProvider {
            public value = 42;
            constructor() {
            }
            $get() {
                return this.value;
            }
        }
        module.provider("a", AProvider)
        class BProvider implements IServiceProvider {
            private aProvider: AProvider;
            constructor($injector: auto.IInjectorService) {
                this.aProvider = $injector.get<AProvider>("aProvider");
            }
            $get() {
                return this.aProvider.value;
            }
        }
        module.provider("b", BProvider);

        var injector = createInjector(["myModule"]);

        expect(injector.get("b")).toBe(42);
    });

    it("allows injecting the $provide service to providers", () => {
        class AProvider implements IServiceProvider {
            constructor($provide: auto.IProvideService) {
                $provide.constant("b", 2);
            }
            $get(b: number) {
                return b + 40;
            }
        }

        module.provider("a", AProvider)

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("does not allow injecting the $provide service to $get", () => {
        class AProvider implements IServiceProvider {
            constructor() {
            }
            $get($provide: auto.IProvideService) {
                return 0;
            }
        }

        module.provider("a", AProvider)

        var injector = createInjector(["myModule"]);

        expect(function () {
            injector.get("a");
        }).toThrow();
    });
});
