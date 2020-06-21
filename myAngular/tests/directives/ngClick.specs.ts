"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IControllerService, auto, IControllerProvider, ICompileProvider, ICompileService, IScope, IAttributes } from "angular";
import $ from "jquery";

describe("ngController", () => {
    var $compile: ICompileService;
    var $rootScope: IScope;
    beforeEach(function () {
        publishExternalAPI();
        var injector = createInjector(["ng"]);
        $compile = injector.get("$compile");
        $rootScope = injector.get("$rootScope");
    });

    afterEach(() => {
        delete window.angular;
    });

    it("starts a digest on ngClick", () => {
        var watchSpy = jasmine.createSpy("watchSpy");
        $rootScope.$watch(watchSpy);

        var button = $("<button ng-click='doSomethig()'></button>");
        $compile(button)($rootScope);
        button.click();
        expect(watchSpy).toHaveBeenCalled();
    });

    it("evaluates given expression on click", () => {
        $rootScope.doSomething = jasmine.createSpy("doSomething");
        var button = $("<button ng-click='doSomething()'></button>");
        $compile(button)($rootScope);
        button.click();
        expect($rootScope.doSomething).toHaveBeenCalled();
    });
    
    it("passes $event to expression", () => {
        var doSomething = jasmine.createSpy("doSomething");
        $rootScope.doSomething = doSomething;
        var button = $("<button ng-click='doSomething($event)'></button>");
        $compile(button)($rootScope);
        button.click();
        var evt: Event  = doSomething.calls.mostRecent().args[0];
        expect(evt).toBeDefined();
        expect(evt.type).toBe("click");
        expect(evt.target).toBeDefined();
    });
});