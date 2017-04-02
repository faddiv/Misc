import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { auto } from "angular";

describe("angularPublic", () => {
    "use strict";
    var injector: auto.IInjectorService;
    beforeEach(() => {
        publishExternalAPI();
        injector = createInjector(["ng"]);
    });

    it("sets up the angular object and module loader", () => {

        expect(window.angular).toBeDefined();
        expect(window.angular.module).toBeDefined();
    });

    it("sets up the ng module", () => {
        expect(injector).toBeDefined();
    });

    it("sets up the $filter service", () => {
        expect(injector.has("$filter")).toBe(true);
        expect(injector.get("$filter")).toBeDefined();
    });

    it("sets up the $parse service", () => {
        var parse = injector.get("$parse");
        expect(parse).toBeDefined();
    });

    it("sets up the $rootScope", () => {
        var $rootScope = injector.get("$rootScope");
        expect($rootScope).toBeDefined();
    });

    it("sets up $q", () => {
        var q = injector.get("$q");
        expect(q).toBeDefined();
    });
    it("sets up $httpBackend", () => {
        var $httpBackend = injector.get("$httpBackend");
        expect($httpBackend).toBeDefined();
    });
    
    it("sets up $http", () => {
        var $http = injector.get("$http");
        expect($http).toBeDefined();
    });
});
