import { setupModuleLoader } from '../../src/loader';
import { IAngularStatic } from "angular";

describe("setupModuleLoader", () => {

    beforeEach(() => {
        delete window.angular;
    });

    afterEach(() => {
        delete window.angular;
    });

    it("exposes angular on the window", () => {
        setupModuleLoader(window);
        expect(window.angular).toBeDefined()
    });

    it("creates angular just once", () => {
        setupModuleLoader(window);
        var ng = window.angular;
        setupModuleLoader(window);
        expect(window.angular).toBe(ng);
    });

    it("exposes the angular module function", () => {
        setupModuleLoader(window);
        expect(window.angular.module).toBeDefined();
    });

    it("exposes the angular module function just once", () => {
        setupModuleLoader(window);
        var module = window.angular.module;
        setupModuleLoader(window);
        expect(window.angular.module).toBe(module);
    });

    describe("modules", () => {
        var ng: IAngularStatic;
        beforeEach(() => {
            setupModuleLoader(window);
            ng = window.angular;
        });

        it("allows registering a module", () => {
            var myModule = ng.module("myModule", []);
            expect(myModule).toBeDefined();
            expect(myModule.name).toEqual("myModule");
        });

        it("replaces a module when registered with same name again", () => {
            var myModule = ng.module("myModule", []);
            var myNewModule = ng.module("myModule", []);
            expect(myNewModule).not.toBe(myModule);
        });

        it("attaches the requires array to the registered module", () => {
            var myModule = ng.module("myModule", ["myOtherModule"]);
            expect(myModule.requires).toEqual(["myOtherModule"]);
        });

        it("allows getting a module", () => {
            var myModule = ng.module("myModule", []);
            var gotModule = ng.module("myModule");
            expect(gotModule).toBeDefined();
            expect(gotModule).toBe(myModule);
        });

        it("throws when trying to get a nonexistent module", () => {
            expect(function () {
                ng.module("nonexistentModule");
            }).toThrow();
        });

        it("does not allow a module to be called hasOwnProperty", () => {
            expect(function () {
                ng.module("hasOwnProperty", []);
            }).toThrow();
        });
    });
});