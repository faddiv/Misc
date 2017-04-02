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

    it("can create a deferred", () => {
        var d = $q.defer();
        expect(d).toBeDefined();
    });

    it("has a promise for each deferred", () => {
        var d = $q.defer();
        expect(d.promise).toBeDefined();
    });

    it("can resolve a promise", (done: Function) => {
        var deferred = $q.defer();
        var promise = deferred.promise;

        var promiseSpy = jasmine.createSpy("promiseSpy");
        promise.then(promiseSpy);

        deferred.resolve("a-ok");

        setTimeout(function () {
            expect(promiseSpy).toHaveBeenCalledWith("a-ok");
            done();
        }, 1);
    });

    it("works when resolved before promise listener", (done) => {
        var deferred = $q.defer();
        var promise = deferred.promise;

        deferred.resolve("a-ok");

        var promiseSpy = jasmine.createSpy("promiseSpy");
        promise.then(promiseSpy);

        setTimeout(function () {
            expect(promiseSpy).toHaveBeenCalledWith("a-ok");
            done();
        }, 1);
    });

    it("does not resolve promise immediately", () => {
        var deferred = $q.defer();
        var promise = deferred.promise;

        var promiseSpy = jasmine.createSpy("promiseSpy");
        promise.then(promiseSpy);

        deferred.resolve("a-ok");

        expect(promiseSpy).not.toHaveBeenCalledWith("a-ok");
    });

    it("resolves promise at next digest", () => {
        var d = $q.defer();

        var promiseSpy = jasmine.createSpy("promiseSpy");
        d.promise.then(promiseSpy);

        d.resolve(42);
        $rootScope.$apply(function () { });

        expect(promiseSpy).toHaveBeenCalledWith(42);
    });

    it("may only be resolved once", () => {
        var d = $q.defer();

        var promiseSpy = jasmine.createSpy("promiseSpy");
        d.promise.then(promiseSpy);

        d.resolve(42);
        d.resolve(43);

        $rootScope.$apply(function () { });

        expect(promiseSpy.calls.count()).toEqual(1);
        expect(promiseSpy).toHaveBeenCalledWith(42);
    });

    it("may only ever be resolved once", () => {
        var d = $q.defer();

        var promiseSpy = jasmine.createSpy("promiseSpy");
        d.promise.then(promiseSpy);

        d.resolve(42);
        $rootScope.$apply(function () { });
        expect(promiseSpy).toHaveBeenCalledWith(42);

        d.resolve(43);
        $rootScope.$apply(function () { });
        expect(promiseSpy.calls.count()).toEqual(1);
    });

    it("resolves a listener added after resolution", () => {
        var d = $q.defer();
        d.resolve(42);
        $rootScope.$apply(function () { });

        var promiseSpy = jasmine.createSpy("promiseSpy");
        d.promise.then(promiseSpy);

        $rootScope.$apply(function () { });

        expect(promiseSpy).toHaveBeenCalledWith(42);
    });

    it("may have multiple callbacks", () => {
        var d = $q.defer();

        var firstSpy = jasmine.createSpy("firstSpy");
        var secondSpy = jasmine.createSpy("secondSpy");
        d.promise.then(firstSpy);
        d.promise.then(secondSpy);

        d.resolve(42);
        $rootScope.$apply(function () { });

        expect(firstSpy).toHaveBeenCalledWith(42);
        expect(secondSpy).toHaveBeenCalledWith(42);
    });

    it("invokes each callbacks once", () => {
        var d = $q.defer();

        var firstSpy = jasmine.createSpy("firstSpy");
        var secondSpy = jasmine.createSpy("secondSpy");
        d.promise.then(firstSpy);
        d.resolve(42);
        $rootScope.$apply(function () { });
        expect(firstSpy.calls.count()).toEqual(1);
        expect(secondSpy.calls.count()).toEqual(0);

        d.promise.then(secondSpy);
        expect(firstSpy.calls.count()).toEqual(1);
        expect(secondSpy.calls.count()).toEqual(0);

        $rootScope.$apply(function () { });
        expect(firstSpy.calls.count()).toEqual(1);
        expect(secondSpy.calls.count()).toEqual(1);
    });
    //Additional test for a missing details.
    it("execute the callback when registered in another then callback", () => {
        var d = $q.defer();

        var promiseSpy = jasmine.createSpy("promiseSpy");
        d.promise.then(function () {
            d.promise.then(promiseSpy);
        });
        d.resolve(42);
        $rootScope.$apply(function () { });
        expect(promiseSpy.calls.count()).toEqual(1);
    });

    it("can reject a deferred", () => {
        var d = $q.defer();

        d.promise.then(fulfilledSpy, rejectedSpy);
        d.reject("fail");
        $rootScope.$apply(function () { });
        expect(fulfilledSpy.calls.count()).toEqual(0);
        expect(rejectedSpy.calls.count()).toEqual(1);
    });

    it("can reject just once", () => {
        var d = $q.defer();

        d.promise.then(fulfilledSpy, rejectedSpy);
        d.reject("fail");
        $rootScope.$apply(function () { });
        expect(rejectedSpy.calls.count()).toEqual(1);

        d.reject("fail 2");
        $rootScope.$apply(function () { });
        expect(rejectedSpy.calls.count()).toEqual(1);
    });

    it("cannnot fulfill a promise once rejeted", () => {
        var d = $q.defer();

        d.promise.then(fulfilledSpy, rejectedSpy);
        d.reject("fail");
        $rootScope.$apply(function () { });
        d.resolve("success");
        $rootScope.$apply(function () { });

        expect(rejectedSpy.calls.count()).toEqual(1);
        expect(fulfilledSpy.calls.count()).toEqual(0);
    });

    it("cannnot rejext a promise once fulfilled", () => {
        var d = $q.defer();

        d.promise.then(fulfilledSpy, rejectedSpy);
        d.resolve("success");
        $rootScope.$apply(function () { });
        d.reject("fail");
        $rootScope.$apply(function () { });

        expect(rejectedSpy.calls.count()).toEqual(0);
        expect(fulfilledSpy.calls.count()).toEqual(1);
    });

    it("does not require a success handler each time", () => {
        var d = $q.defer();

        d.promise.then(null, rejectedSpy);
        d.promise.then(fulfilledSpy);

        d.resolve("ok");
        $rootScope.$apply(function () { });
        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
    });

    it("does not require a reject handler each time", () => {
        var d = $q.defer();

        d.promise.then(fulfilledSpy);
        d.promise.then(null, rejectedSpy);

        d.reject("bad");
        $rootScope.$apply(function () { });
        expect(rejectedSpy).toHaveBeenCalledWith("bad");
    });

    it("can register rejection handler with catch", () => {
        var d = $q.defer();

        d.promise.catch(rejectedSpy);
        d.reject("fail");
        $rootScope.$apply(function () { });
        expect(rejectedSpy.calls.count()).toEqual(1);
    });

    it("invokes a finally handler when fulfilled", () => {
        var d = $q.defer();

        d.promise.finally(finallySpy);
        d.resolve(42);
        $rootScope.$apply(function () { });

        expect(finallySpy).toHaveBeenCalledWith();
    });

    it("invokes a finally handler when rejected", () => {
        var d = $q.defer();

        d.promise.finally(finallySpy);
        d.reject(42);
        $rootScope.$apply(function () { });

        expect(finallySpy).toHaveBeenCalledWith();
    });

    it("allows chaining handlers", () => {
        var d = $q.defer<number>();

        d.promise.then(function (result) {
            return result + 1;
        }).then(function (result) {
            return result * 2;
        }).then(fulfilledSpy);
        d.resolve(20);
        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith(42);
    });

    it("does not modify original resolution in chains", () => {
        var d = $q.defer<number>();

        d.promise.then(function (result) {
            return result + 1;
        });
        d.promise.then(function (result) {
            return result * 2;
        });
        d.promise.then(fulfilledSpy);
        d.resolve(20);
        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith(20);
    });

    it("catches rejection on chained handler", () => {
        var d = $q.defer();

        d.promise.then(_.noop).catch(rejectedSpy);

        d.reject("fail");
        $rootScope.$apply(function () { });

        expect(rejectedSpy).toHaveBeenCalledWith("fail");
    });

    it("fulfills on chained handler", () => {
        var d = $q.defer();

        d.promise.catch(_.noop).then(fulfilledSpy);

        d.resolve("ok");
        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
    });

    it("treats catch return value as resolution", () => {
        var d = $q.defer();

        d.promise.catch(function () { return 42; }).then(fulfilledSpy);

        d.reject("fail");
        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith(42);
    });

    it("rejects chained promise when handler throws", () => {
        var d = $q.defer();

        d.promise.then(function () { throw "fail"; }).catch(rejectedSpy);

        d.resolve(42);
        $rootScope.$apply(function () { });

        expect(rejectedSpy).toHaveBeenCalledWith("fail");
    });

    it("does not reject current promise when handler throws", () => {
        var d = $q.defer();

        d.promise.then(function () { throw "fail"; });
        d.promise.catch(rejectedSpy);
        d.resolve(42);
        $rootScope.$apply(function () { });

        expect(rejectedSpy).not.toHaveBeenCalled();
    });

    it("waits on promise returned from handler", () => {
        var d = $q.defer<number>();

        d.promise.then(function (v) {
            var d2 = $q.defer();
            d2.resolve(v + 1);
            return d2.promise;
        }).then(function (v: number) {
            return v * 2;
        }).then(fulfilledSpy);

        d.resolve(20);
        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith(42);
    });

    it("waits on promise given to resolve", () => {
        var d = $q.defer();
        var d2 = $q.defer();

        d.promise.then(fulfilledSpy);
        d2.resolve(42);
        d.resolve(d2.promise);

        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith(42);
    });

    it("rejects when promise returned from handler rejects", () => {
        var d = $q.defer();
        d.promise.then(function () {
            var d2 = $q.defer();
            d2.reject("fail");
            return d2.promise;
        }).catch(rejectedSpy);
        d.resolve("ok");

        $rootScope.$apply(function () { });

        expect(rejectedSpy).toHaveBeenCalledWith("fail");
    });

    it("allows chaining handlers on finally, with original value", () => {
        var d = $q.defer<number>();

        d.promise.then(function (result) {
            return result + 1;
        }).finally(function () {
            return arguments[0] + 1;
        }).then(fulfilledSpy);
        d.resolve(20);

        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith(21);
    });

    it("allows chaining handlers on finally, with original rejection", () => {
        var d = $q.defer<number>();

        d.promise.then(function (result) {
            throw "fail";
        }).finally(function () {
        }).catch(rejectedSpy);
        d.resolve(20);

        $rootScope.$apply(function () { });

        expect(rejectedSpy).toHaveBeenCalledWith("fail");
    });

    it("resolves to original value when nested promise resolves", () => {
        var d = $q.defer<number>();

        var resolveNested;
        d.promise.then(function (result) {
            return result + 1;
        }).finally(function () {
            var d2 = $q.defer();
            resolveNested = function () {
                d2.resolve("abc");
            };
            return d2.promise;
        }).then(fulfilledSpy);
        d.resolve(20);

        $rootScope.$apply(function () { });
        expect(fulfilledSpy).not.toHaveBeenCalled();

        resolveNested();

        $rootScope.$apply(function () { });
        expect(fulfilledSpy).toHaveBeenCalledWith(21);
    });

    it("rejects to original value when nested promise resolves", () => {
        var d = $q.defer<number>();

        var resolveNested;
        d.promise.then(function (result) {
            throw "fail"
        }).finally(function () {
            var d2 = $q.defer();
            resolveNested = function () {
                d2.resolve("abc");
            };
            return d2.promise;
        }).catch(rejectedSpy);
        d.resolve(20);

        $rootScope.$apply(function () { });
        expect(rejectedSpy).not.toHaveBeenCalled();

        resolveNested();

        $rootScope.$apply(function () { });
        expect(rejectedSpy).toHaveBeenCalledWith("fail");
    });

    it("rejectes when nested promise rejects in finally", () => {
        var d = $q.defer<number>();

        var resolveNested;
        d.promise.then(function (result) {
            return result + 1;
        }).finally(function () {
            var d2 = $q.defer();
            resolveNested = function () {
                d2.reject("fail");
            };
            return d2.promise;
        }).then(fulfilledSpy, rejectedSpy);
        d.resolve(20);

        $rootScope.$apply(function () { });
        expect(fulfilledSpy).not.toHaveBeenCalled();
        expect(rejectedSpy).not.toHaveBeenCalled();

        resolveNested();

        $rootScope.$apply(function () { });
        expect(fulfilledSpy).not.toHaveBeenCalled();
        expect(rejectedSpy).toHaveBeenCalledWith("fail");
    });

    it("can report progress", () => {
        var d = $q.defer();

        d.promise.then(null, null, progressSpy);
        d.notify("working...");
        $rootScope.$apply(function () { });

        expect(progressSpy).toHaveBeenCalledWith("working...");
    });

    it("can report progress many times", () => {
        var d = $q.defer();
        d.promise.then(null, null, progressSpy);
        d.notify("40%");
        $rootScope.$apply(function () { });

        d.notify("80%");
        d.notify("100%");
        $rootScope.$apply(function () { });

        expect(progressSpy.calls.count()).toBe(3);
    });

    it("does not notify progress after being resolved", () => {
        var d = $q.defer();
        d.promise.then(null, null, progressSpy);

        d.resolve("ok");
        d.notify("working...");
        $rootScope.$apply(function () { });

        expect(progressSpy).not.toHaveBeenCalled();
    });

    it("does not notify progress after being rejected", () => {
        var d = $q.defer();
        d.promise.then(null, null, progressSpy);

        d.reject("not ok");
        d.notify("working...");
        $rootScope.$apply(function () { });

        expect(progressSpy).not.toHaveBeenCalled();
    });

    it("can notify progress through chained", () => {
        var d = $q.defer();

        d.promise
            .then(_.noop)
            .catch(_.noop)
            .then(null, null, progressSpy);

        d.notify("working...");
        $rootScope.$apply(function () { });

        expect(progressSpy).toHaveBeenCalledWith("working...");
    });

    it("transforms progress through handlers", () => {
        var d = $q.defer();

        d.promise
            .then(_.noop)
            .then(null, null, function (progress) {
                return "..." + progress;
            })
            .catch(_.noop)
            .then(null, null, progressSpy);

        d.notify("working...");
        $rootScope.$apply(function () { });

        expect(progressSpy).toHaveBeenCalledWith("...working...");
    });

    it("recovers from progressback exceptions", () => {
        var d = $q.defer();

        d.promise
            .then(null, null, function (progress) {
                throw "fail";
            });
        d.promise
            .then(fulfilledSpy, null, progressSpy);

        d.notify("working...");
        d.resolve("ok");
        $rootScope.$apply(function () { });

        expect(progressSpy).toHaveBeenCalledWith("working...");
        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
    });

    it("can notify progress through promise returned from handler", () => {
        var d = $q.defer();
        d.promise.then(null, null, progressSpy);

        var d2 = $q.defer();
        d.resolve(d2.promise);
        d2.notify("working...");

        $rootScope.$apply(function () { });

        expect(progressSpy).toHaveBeenCalledWith("working...");
    });

    it("allows attaching progressback in finally", () => {
        var d = $q.defer();
        d.promise.finally(null, progressSpy);

        d.notify("working...");
        $rootScope.$apply(function () { });

        expect(progressSpy).toHaveBeenCalledWith("working...");
    });

    it("can make an immediately rejected promise", () => {

        var promise = $q.reject("fail");
        promise.then(fulfilledSpy, rejectedSpy);

        $rootScope.$apply(function () { });

        expect(fulfilledSpy).not.toHaveBeenCalled();
        expect(rejectedSpy).toHaveBeenCalledWith("fail");
    });

    it("can make an immediately resolved promise", () => {

        var promise = $q.resolve("ok");
        promise.then(fulfilledSpy, rejectedSpy);

        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
        expect(rejectedSpy).not.toHaveBeenCalled();
    });

    it("can make an immediately resolved promise with when", () => {

        var promise = $q.when("ok");
        promise.then(fulfilledSpy, rejectedSpy);

        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
        expect(rejectedSpy).not.toHaveBeenCalled();
    });

    it("can wrap a foreign promise", () => {

        var promise = $q.when({
            then(handler: Function) {
                $rootScope.$evalAsync(function () {
                    handler("ok");
                });
            }
        });

        promise.then(fulfilledSpy, rejectedSpy);

        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
        expect(rejectedSpy).not.toHaveBeenCalled();
    });

    it("takes callbacks directly when wrapping", () => {

        var wrapped = $q.defer();
        $q.when(wrapped.promise, fulfilledSpy, rejectedSpy, progressSpy);

        wrapped.notify("working...")
        wrapped.resolve("ok");
        $rootScope.$apply(function () { });

        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
        expect(rejectedSpy).not.toHaveBeenCalled();
        expect(progressSpy).toHaveBeenCalledWith("working...");
    });
});