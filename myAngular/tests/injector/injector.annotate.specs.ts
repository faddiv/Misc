import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IAngularStatic, auto } from "angular";

describe("setupModuleLoader", () => {
    var ng: IAngularStatic;
    var injector: auto.IInjectorService;
    beforeEach(() => {
        delete window.angular;
        setupModuleLoader(window);
        ng = window.angular;
        injector = createInjector([]);
    });

    afterEach(() => {
        delete window.angular;
    });

    describe("annotate", () => {
        it("returns the $inject annotation of a function when it has one", () => {
            var fn = function () { }
            fn.$inject = ["a", "b"];

            expect(injector.annotate(fn)).toEqual(["a", "b"]);
        });

        it("returns the array-style annotations of a function", () => {
            var fn = ["a", "b", function () { }];

            expect(injector.annotate(fn)).toEqual(["a", "b"]);
        });
    });

    it("returns an empty array for a non-annotated 0-arg function", () => {
        var fn = function () { };
        expect(injector.annotate(fn)).toEqual([]);
    });

    it("returns annotations parsed from function args when not annotated", () => {

        var fn = function (a, b) { }

        expect(injector.annotate(fn)).toEqual(["a", "b"]);
    });

    it("strips comments from argument lists when parsing", () => {
        var fn = function (a, /*b,*/ c) { };
        var orig = fn.prototype.toString;
        try {
            fn.toString = function () { return "function (a, /*b,*/ c) { }" };
            expect(injector.annotate(fn)).toEqual(["a", "c"]);
        } finally {
            fn.toString = orig;
        }
    });

    it("strips several comments from argument lists when parsing", () => {
        var fn = function (a, /*b,*/ c/*, d*/) { };
        var orig = fn.prototype.toString;
        try {
            fn.toString = function () { return "function (a, /*b,*/ c/*, d*/) { }" };
            expect(injector.annotate(fn)).toEqual(["a", "c"]);
        } finally {
            fn.toString = orig;
        }
    });

    it("strips // comments from argument lists when parsing", () => {
        var fn = function (a, //b,
            c) { };
        var orig = fn.prototype.toString;
        try {
            fn.toString = function () { return "function (a, //b,\r\n c) { };" };
            expect(injector.annotate(fn)).toEqual(["a", "c"]);
        } finally {
            fn.toString = orig;
        }
    });

    it("strips surrounding undersores from argument names when parsing", () => {
        var fn = function (a, _b_, c_, _d, an_argument) { };
        expect(injector.annotate(fn)).toEqual(["a", "b", "c_", "_d", "an_argument"]);
    });

    it("throws when using a non-annotated fn in strick mode", () => {
        var injector = createInjector([], true);

        var fn = function(a, b, c) {  };

        expect(()=>{
            injector.annotate(fn);
        }).toThrow();
    });
});
