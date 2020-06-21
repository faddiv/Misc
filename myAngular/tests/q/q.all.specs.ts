import * as _ from 'lodash';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("$q", () => {
    var $q: IQService;
    var $rootScope: IRootScopeService;
    var fulfilledSpy: (result: any) => any;
    var rejectedSpy: (result: any) => any;
    beforeEach(() => {
        publishExternalAPI();
        var injector = createInjector(["ng"]);
        $q = injector.get("$q");
        $rootScope = injector.get("$rootScope");
        fulfilledSpy = jasmine.createSpy("fulfilledSpy");
        rejectedSpy = jasmine.createSpy("rejectedSpy");
    });
    function applyRootScope() {
        $rootScope.$apply(function () { });
    }
    describe("all", () => {
        it("can resolve an array of promises to array of results", () => {
            var p1 = $q.when(1);
            var p2 = $q.when(2);
            var p3 = $q.when(3);
            var promise = $q.all([p1, p2, p3]);
            promise.then(fulfilledSpy);

            applyRootScope();
            expect(fulfilledSpy).toHaveBeenCalledWith([1, 2, 3]);
        });

        it("can resolve an object of promises to an object of results", () => {
            var p1 = $q.when(1);
            var p2 = $q.when(2);
            var p3 = $q.when(3);
            var promise = $q.all({ a: p1, b: p2, c: p3 });
            promise.then(fulfilledSpy);
            applyRootScope();
            expect(fulfilledSpy).toHaveBeenCalledWith({ a: 1, b: 2, c: 3 });
        });

        it("resolves an empty array of promises immediately", () => {
            var promise = $q.all([]);
            promise.then(fulfilledSpy);

            applyRootScope();
            expect(fulfilledSpy).toHaveBeenCalledWith([]);
        });

        it("resolves an empty object of promises immediately", () => {
            var promise = $q.all({});
            promise.then(fulfilledSpy);

            applyRootScope();
            expect(fulfilledSpy).toHaveBeenCalledWith({});
        });

        it("rejects when any of the promises rejects", () => {
            var p1 = $q.when(1);
            var p2 = $q.when(2);
            var p3 = $q.reject("fail");
            var promise = $q.all([p1, p2, p3]);
            promise.then(fulfilledSpy, rejectedSpy);

            applyRootScope();

            expect(fulfilledSpy).not.toHaveBeenCalled();
            expect(rejectedSpy).toHaveBeenCalledWith("fail");
        });

        it("wraps non-promises in the input collection", () => {
            var p1 = $q.when(1);
            var promise = $q.all([p1, 2, 3]);
            promise.then(fulfilledSpy);

            applyRootScope();

            expect(fulfilledSpy).toHaveBeenCalledWith([1, 2, 3]);
        });
    });
});