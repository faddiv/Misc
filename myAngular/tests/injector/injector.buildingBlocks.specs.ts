import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IServiceProvider, IModule, IAngularStatic, auto } from "angular";

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

    it("run config blocks when the injector is created", () => {
        var hasRun = false;
        module.config(function () {
            hasRun = true;
        })

        createInjector(["myModule"]);

        expect(hasRun).toBe(true);
    });

    it("injects config blocks with provider injector", () => {
        module.config(function ($provide: auto.IProvideService) {
            $provide.constant("a", 42);
        });

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("injects config blocks with array mode", () => {
        module.config(["$provide", function ($p: auto.IProvideService) {
            $p.constant("a", 42);
        }]);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("injects config blocks with with $inject array mode", () => {
        function Config($provide: auto.IProvideService) {
            $provide.constant("a", 42);
        }
        Config.$inject = ["$provide"];
        module.config(Config);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("allows registering config blocks before providers", () => {
        module.config(function (aProvider: AProvider) {
            aProvider.value = 42;
        });
        class AProvider implements IServiceProvider {
            public value = 0;
            constructor() {

            }
            $get() {
                return this.value;
            }
        }
        module.provider("a", AProvider);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("runs a config block added during module registration", () => {
        var module2 = ng.module("myOtherModule", [], function ($provide: auto.IProvideService) {
            $provide.constant("a", 42);
        });

        var injector = createInjector(["myOtherModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("runs run blocks when the injector is created", () => {
        var hasRun = false;
        module.run(function () {
            hasRun = true;
        });
        createInjector(["myModule"]);
        expect(hasRun).toBe(true);
    });

    it("injects run blocks with the instance injector", () => {
        var gotA: number;
        module.run(function (a: number) {
            gotA = a;
        });

        class AProvider implements IServiceProvider {
            public value = 42;
            constructor() {

            }
            $get() {
                return this.value;
            }
        }
        module.provider("a", AProvider);


        createInjector(["myModule"]);

        expect(gotA).toBe(42);
    });

    it("configures all modules before running any run blocks", () => {
        var result: number;
        class AProvider implements IServiceProvider {
            constructor() {

            }
            $get() {
                return 2;
            }
        }
        module.provider("a", AProvider);
        module.run(function (a, b) {
            result = a + b;
        });

        var myOtherModule = ng.module("myOtherModule", []);
        class BProvider implements IServiceProvider {
            constructor() {

            }
            $get() {
                return 40;
            }
        }
        myOtherModule.provider("b", BProvider);

        createInjector(["myModule", "myOtherModule"]);

        expect(result).toBe(42);
    });
    //Function modules
    it("runs a function module dependency as a config block", () => {
        var functionModule = function ($provide: auto.IProvideService) {
            $provide.constant("a", 42);
        };
        ng.module("myModule", [<any>functionModule]);

        var injector = createInjector(["myModule"]);

        expect(injector.get("a")).toBe(42);
    });

    it("runs a function module with array injection as a config block", () => {
        var functionModule = ["$provide", function (p: auto.IProvideService) {
            p.constant("a", 42);
        }];
        ng.module("myModule", [<any>functionModule]);

        var injector = createInjector(["myModule"]);
        expect(injector.get("a")).toBe(42);
    });

    it("supports returning a run block from a function module", () => {
        var result;
        var functionModule = function ($provide: auto.IProvideService) {
            $provide.constant("a", 42);
            return function (a) {
                result = a;
            }
        };
        ng.module("myModule", [<any>functionModule]);
        createInjector(["myModule"]);

        expect(result).toBe(42);
    });
    
    it("only loads function modules once", () => {
        var loadTimes = 0;
        var functionModule = function () {
            loadTimes++;
        };
        ng.module("myModule", [<any>functionModule, <any>functionModule]);
        createInjector(["myModule"]);
        expect(loadTimes).toBe(1);
    });
});
