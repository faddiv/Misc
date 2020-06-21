import * as _ from 'lodash';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("$q", () => {
    var $q: IQService;
    var $rootScope: IRootScopeService;
    var fulfilledSpy: jasmine.Spy;
    var rejectedSpy: jasmine.Spy;
    var finallySpy: jasmine.Spy;
    var progressSpy: jasmine.Spy;
    beforeEach(() => {
        publishExternalAPI();
        var injector = createInjector(["ng"]);
        $q = injector.get("$q");
        $rootScope = injector.get("$rootScope");
        fulfilledSpy = jasmine.createSpy("fulfilledSpy");
        rejectedSpy = jasmine.createSpy("rejectedSpy");
        finallySpy = jasmine.createSpy("finallySpy");
        progressSpy = jasmine.createSpy("progressSpy");
    });

    function applyRootScope() {
        $rootScope.$apply(function () { });
    }

    describe("ES2015 style", () => {

        it("is a function", () => {
            expect($q instanceof Function).toBe(true);
        });

        it("expects a function as an argument", () => {
            expect($q).toThrow();
            expect(function () { $q(_.noop); }).not.toThrow();
            expect(function () { $q(<any>{}); }).toThrow();
        });

        it("returns a promise", () => {
            var p = $q(_.noop);
            expect(p).toBeDefined();
            expect(p.then).toBeDefined();
        });

        it("calls function with a resolve function", () => {
            $q(function (resolve) {
                resolve("ok");
            }).then(fulfilledSpy);

            applyRootScope();

            expect(fulfilledSpy).toHaveBeenCalledWith("ok");
        });

        it("calls function with a reject function", () => {
            $q(function (resolve, reject) {
                reject("fail");
            }).then(fulfilledSpy, rejectedSpy);

            applyRootScope();

            expect(fulfilledSpy).not.toHaveBeenCalled();
            expect(rejectedSpy).toHaveBeenCalledWith("fail");
        });
    });
});