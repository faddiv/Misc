import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IAngularStatic } from "angular";

describe("setupModuleLoader", () => {
    var ng: IAngularStatic;

    beforeEach(() => {
        delete window.angular;
        setupModuleLoader(window);
        ng = window.angular;
    });

    afterEach(() => {
        delete window.angular;
    });

    it("invokes an array-annotated function with dependency injection", () => {
        var module = ng.module("myModule", []);
        module.constant("a", 40);
        module.constant("b", 2);
        var injector = createInjector(["myModule"]);

        var fn = ["a", "b", function (one, two) { return one + two; }];

        expect(injector.invoke(fn)).toBe(42);
    });

    it("invokes a non-annotated function with dependency injection", () => {
        var module = ng.module("myModule", []);
        module.constant("one", 40);
        module.constant("two", 2);
        var injector = createInjector(["myModule"]);

        var fn = function (one, two) { return one + two; };

        expect(injector.invoke(fn)).toBe(42);
    });

    it("instantiates an annotated constructor function", () => {
        var module = ng.module("myModule", []);
        module.constant("a", 40);
        module.constant("b", 2);
        var injector = createInjector(["myModule"]);

        class Type {
            public result;
            static $inject = ["a", "b"];
            constructor(one, two) {
                this.result = one + two;
            }
        }
        var instance = injector.instantiate<Type>(Type);
        expect(instance.result).toBe(42);
    });

    it("instantiates an array-annotated constructor function", () => {
        var module = ng.module("myModule", []);
        module.constant("a", 40);
        module.constant("b", 2);
        var injector = createInjector(["myModule"]);

        class Type {
            public result;
            constructor(one, two) {
                this.result = one + two;
            }
        }
        var instance = injector.instantiate<Type>(<any>["a", "b", Type]);
        expect(instance.result).toBe(42);
    });

    it("instantiates a non-annotated constructor function", () => {
        var module = ng.module("myModule", []);
        module.constant("one", 40);
        module.constant("two", 2);
        var injector = createInjector(["myModule"]);

        class Type {
            public result;
            constructor(one, two) {
                this.result = one + two;
            }
        }
        var instance = injector.instantiate<Type>(Type);
        expect(instance.result).toBe(42);
    });

    it("uses the prototype of the constructor when instantiating", () => {
        var module = ng.module("myModule", []);
        module.constant("one", 40);
        module.constant("two", 2);
        var injector = createInjector(["myModule"]);

        class Type {
            public result;
            constructor() {
                this.result = this.getValue();
            }
            public getValue() {
                return 42;
            }
        }
        var instance = injector.instantiate<Type>(Type);
        expect(instance.result).toBe(42);
    });

    it("supports locals when instantiating", () => {
        var module = ng.module("myModule", []);
        module.constant("one", 40);
        module.constant("two", 2);
        var injector = createInjector(["myModule"]);

        class Type {
            public result;
            constructor(one, two) {
                this.result = one + two;
            }
        }
        var instance = injector.instantiate<Type>(Type, { two: 3 });
        expect(instance.result).toBe(43);
    });
});
