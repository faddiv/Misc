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

    it("can be created", () => {
        var injector = createInjector([]);
        expect(injector).toBeDefined();
    });

    it("has a constant that has been registered to a module", () => {
        var module = ng.module("myModule", []);
        module.constant("aConstant", 42);
        var injector = createInjector(["myModule"]);
        expect(injector.has("aConstant")).toBe(true);
    });

    it("does not have a non-registered constant", () => {
        var module = ng.module("myModule", []);
        var injector = createInjector(["myModule"]);
        expect(injector.has("aConstant")).toBe(false);
    });

    it("does not allow a constant called hasOwnProperty", () => {
        var module = ng.module("myModule", []);
        module.constant("hasOwnProperty", false);
        expect(function () {
            createInjector(["myModule"]);
        }).toThrow();
    });

    it("can return a registered constant", () => {
        var module = ng.module("myModule", []);
        module.constant("aConstant", 42);
        var injector = createInjector(["myModule"]);
        expect(injector.get<number>("aConstant")).toBe(42);
    });

    it("loads multiple modules", () => {
        var module1 = ng.module("myModule", []);
        var module2 = ng.module("myOtherModule", []);
        module1.constant("aConstant", 42);
        module2.constant("anotherConstant", 43);
        var injector = createInjector(["myModule", "myOtherModule"]);

        expect(injector.has("aConstant")).toBe(true);
        expect(injector.has("anotherConstant")).toBe(true);
    });

    it("loads the required modules of a module", () => {
        var module1 = ng.module("myModule", []);
        var module2 = ng.module("myOtherModule", ["myModule"]);
        module1.constant("aConstant", 42);
        module2.constant("anotherConstant", 43);
        var injector = createInjector(["myOtherModule"]);

        expect(injector.has("aConstant")).toBe(true);
        expect(injector.has("anotherConstant")).toBe(true);
    });

    it("loads the transitively required modules of a module", () => {
        var module1 = ng.module("myModule", []);
        var module2 = ng.module("myOtherModule", ["myModule"]);
        var module3 = ng.module("myThirdModule", ["myOtherModule"]);
        module1.constant("aConstant", 42);
        module2.constant("anotherConstant", 43);
        module3.constant("aThirdConstant", 44);
        var injector = createInjector(["myThirdModule"]);

        expect(injector.has("aConstant")).toBe(true);
        expect(injector.has("anotherConstant")).toBe(true);
        expect(injector.has("aThirdConstant")).toBe(true);
    });

    it("loads each module only once", () => {
        ng.module("myModule", ["myOtherModule"]);
        ng.module("myOtherModule", ["myModule"]);

        createInjector(["myModule"]);
    }, 100);

    it("invokes an annotated function with dependency injection", () => {
        var module = ng.module("myModule", []);
        module.constant("a", 40);
        module.constant("b", 2);
        var injector = createInjector(["myModule"]);

        var fn = function (one, two) { return one + two };
        fn.$inject = ["a", "b"];

        expect(injector.invoke(fn)).toBe(42);
    });

    it("does not accept non-strings as injection tokens", () => {
        var module = ng.module("myModule", []);
        module.constant("a", 1);
        var injector = createInjector(["myModule"]);

        var fn = function (one, two) { return one + two };
        fn.$inject = ["a", <any>2];

        expect(function () {
            injector.invoke(fn);
        }).toThrow();
    });

    it("invokes a function with the given this context", () => {
        var module = ng.module("myModule", []);
        module.constant("a", 1);
        var injector = createInjector(["myModule"]);

        var obj = {
            two: 2,
            fn(one) { return one + this.two; }
        };
        obj.fn.$inject = ["a"];

        expect(injector.invoke(obj.fn, obj));
    });

    it("overrides dependencies with locals when invoking", () => {
        var module = ng.module("myModule", []);
        module.constant("a", 40);
        module.constant("b", 2);
        var injector = createInjector(["myModule"]);

        var fn = function (one, two) { return one + two; };
        fn.$inject = ["a", "b"];
        expect(injector.invoke(fn, undefined, { b: 3 })).toBe(43);
    });
});