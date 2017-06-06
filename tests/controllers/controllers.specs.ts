"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IControllerService, auto, IControllerProvider } from "angular";
import $ from "jquery";

describe("$controller", () => {
    var injector: auto.IInjectorService;
    var $controller: IControllerService;
    beforeEach(function () {
        publishExternalAPI();
    });

    afterEach(() => {
        delete window.angular;
    });

    describe("basics", () => {
        var injector: auto.IInjectorService;
        var $controller: IControllerService;
        beforeEach(function () {
            injector = createInjector(["ng", function ($provide: auto.IProvideService) {
                $provide.constant("aDep", 42);
            }]);
            $controller = injector.get("$controller");

        });
        it("instantiates controller functions", () => {
            class MyController {
                invoked: boolean;
                constructor() {
                    this.invoked = true;
                }
            }

            var controller = $controller(MyController);

            expect(controller).toBeDefined();
            expect(controller instanceof MyController).toBe(true);
            expect(controller.invoked).toBe(true);
        });

        it("injects dependencies to controller functions", () => {
            class MyController {
                static $inject = ["aDep"];
                constructor(public theDep: number) {
                }
            }

            var controller = $controller(MyController);

            expect(controller.theDep).toBe(42);
        });

        it("allows injecting locals to controller functions", () => {
            class MyController {
                static $inject = ["localDep"];
                constructor(public theDep: number) {
                }
            }

            var controller = $controller(MyController, { localDep: 42 });

            expect(controller.theDep).toBe(42);
        });
    });
    //Controller Registration
    it("allows registering controllers at config time", () => {
        class MyController {
            static $inject = ["aDep"];
            constructor(public theDep: number) {
            }
        }
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider) {
            $controllerProvider.register("MyController", MyController);
        }]);
        var $controller = injector.get("$controller");

        var controller = $controller<MyController>("MyController", { aDep: 42 });
        expect(controller).toBeDefined();
        expect(controller instanceof MyController).toBe(true);
        expect(controller.theDep).toBe(42);
    });

    it("allows registering several controllers in an object", () => {
        class MyController {
            static $inject = ["aDep"];
            constructor(public theDep: number) {
            }
        }
        class MyOtherController {
            static $inject = ["bDep"];
            constructor(public theDep: number) {
            }
        }
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider) {
            $controllerProvider.register({
                MyController: MyController,
                MyOtherController: MyOtherController
            });
        }]);
        var $controller = injector.get("$controller");

        var controller = $controller<MyController>("MyController", { aDep: 42 });
        var otherController = $controller<MyOtherController>("MyOtherController", { bDep: 42 });
        expect(controller instanceof MyController).toBe(true);
        expect(otherController instanceof MyOtherController).toBe(true);
    });

    it("allows registering controllers through modules", () => {
        var module = window.angular.module("myModule", []);
        module.controller("MyController", function MyController() { });

        var injector = createInjector(["ng", "myModule"]);
        var $controller = injector.get("$controller");
        var controller = $controller<any>("MyController");

        expect(controller).toBeDefined();
    });

    //Global Controller Lookup
    it("does not normally look controllers up from window", () => {
        window["MyController"] = function MyController() { };
        var injector = createInjector(["ng"]);
        var $controller = injector.get("$controller");
        expect(function () {
            $controller("MyController");
        });
        delete window["MyController"];
    });

    it("looks up controllers from window when so configured", () => {
        window["MyController"] = function MyController() { };
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider) {
            $controllerProvider.allowGlobals();
        }]);
        var $controller = injector.get("$controller");
        var controller = $controller("MyController");
        expect(controller).toBeDefined();
        expect(controller instanceof window["MyController"]).toBeDefined();
        delete window["MyController"];
    });
})