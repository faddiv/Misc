import * as _ from 'lodash';
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IAngularStatic, auto, IModule, IServiceProvider } from "angular";

describe("setupModuleLoader", () => {
    var ng: IAngularStatic;
    var injector: auto.IInjectorService;
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

    it("allows registering a provider and uses its $get", () => {
        module.provider("a", {
            $get() { return 42; }
        });
        injector = createInjector(["myModule"]);
        expect(injector.has("a")).toBe(true);
        expect(injector.get("a")).toBe(42);
    });

    it("injects the $get method of a provider", () => {
        module.constant("a", 2);
        module.provider("b", {
            $get(a) {
                return a + 40;
            }
        });
        injector = createInjector(["myModule"]);
        expect(injector.get("b")).toBe(42);
    });

    it("injects the $get method of a provider lazily", () => {
        module.provider("b", {
            $get(a) {
                return a + 40;
            }
        });
        module.provider("a", { $get: _.constant(2) });
        var injector = createInjector(["myModule"]);
        expect(injector.get("b")).toBe(42);
    });

    it("instantiates a dependeny only once", () => {
        module.provider("a", {
            $get() { return {}; }
        });
        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(injector.get("a"));
    });

    it("notifies the user about circular dependency", () => {
        module.provider("a", { $get(b) { } });
        module.provider("b", { $get(c) { } });
        module.provider("c", { $get(a) { } });
        var injector = createInjector(["myModule"]);
        expect(function () {
            injector.get("a");
        }).toThrowError(/Circular dependency found.* a <- c <- b <- a/);
    });

    it("cleans up the circular marker when instantiation fails", () => {
        module.provider("a", {
            $get() {
                throw "Failing instantiation!";
            }
        });

        var injector = createInjector(["myModule"]);

        expect(function () {
            injector.get("a")
        }).toThrow("Failing instantiation!");

        expect(function () {
            injector.get("a")
        }).toThrow("Failing instantiation!");
    });

    it("instantiates a provider if given as a constructor function", () => {
        class AProvider implements IServiceProvider {
            constructor() {

            }
            $get() {
                return 42;
            }
        }
        module.provider("a", AProvider);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("injects the given provider constant function", () => {
        class AProvider implements IServiceProvider {
            constructor(private b: number) {

            }
            $get() {
                return this.b + 2;
            }
        }
        module.constant("b", 40);
        module.provider("a", AProvider);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("injects another provider to a provider constructor function", () => {
        class AProvider implements IServiceProvider {
            private value = 1;
            constructor() { }
            public setValue(v) {
                this.value = v;
            }
            $get() {
                return this.value;
            }
        }
        class BProvider implements IServiceProvider {
            constructor(aProvider: AProvider) {
                aProvider.setValue(42);
            }
            $get() { }
        }
        module.provider("a", AProvider);
        module.provider("b", BProvider);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("does not inject an instance to a provider constructor function", () => {
        class AProvider implements IServiceProvider {
            $get() {
                return 1;
            }
        }
        class BProvider implements IServiceProvider {
            constructor(private a) {
            }
            $get() { return this.a; }
        }
        module.provider("a", AProvider);
        module.provider("b", BProvider);
        expect(function () {
            createInjector(["myModule"]);
        }).toThrow();
    });

    it("does not inject a provider to a $get function", () => {
        class AProvider implements IServiceProvider {
            $get() {
                return 1;
            }
        }
        class BProvider implements IServiceProvider {
            constructor() {
            }
            $get(aProvider: AProvider) { return aProvider; }
        }
        module.provider("a", AProvider);
        module.provider("b", BProvider);
        var injector = createInjector(["myModule"]);
        expect(function () {
            injector.get("b");
        }).toThrow();
    });

    it("does not inject a provider to invoke", () => {
        class AProvider implements IServiceProvider {
            $get() {
                return 1;
            }
        }

        module.provider("a", AProvider);

        var injector = createInjector(["myModule"]);
        expect(function () {
            injector.invoke(function (aProvider: AProvider) { });
        }).toThrow();
    });

    it("does not give access to providers through get", () => {
        class AProvider implements IServiceProvider {
            $get() {
                return 1;
            }
        }

        module.provider("a", AProvider);

        var injector = createInjector(["myModule"]);
        expect(function () {
            injector.get("aProvider");
        }).toThrow();
    });

    it("registers constant first to make them available to providers", () => {
        class AProvider implements IServiceProvider {
            constructor(private b: number) {

            }
            $get() {
                return this.b;
            }
        }
        module.provider("a", AProvider);
        module.constant("b", 42);

        var injector = createInjector(["myModule"]);
        expect(injector.get("a")).toBe(42);
    });
});