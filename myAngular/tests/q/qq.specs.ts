import * as _ from 'lodash';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IQService, IRootScopeService } from "angular";
"use strict";

describe("$$q", () => {
    var $$q: IQService;
    var $rootScope: IRootScopeService;
    var fulfilledSpy: jasmine.Spy;
    var rejectedSpy: jasmine.Spy;
    var finallySpy: jasmine.Spy;
    var progressSpy: jasmine.Spy;
    var clock: jasmine.Clock;
    beforeEach(() => {
        publishExternalAPI();
        var injector = createInjector(["ng"]);
        $$q = injector.get<IQService>("$$q");
        $rootScope = injector.get("$rootScope");
        fulfilledSpy = jasmine.createSpy("fulfilledSpy");
        rejectedSpy = jasmine.createSpy("rejectedSpy");
        finallySpy = jasmine.createSpy("finallySpy");
        progressSpy = jasmine.createSpy("progressSpy");
        clock = jasmine.clock();
        clock.install();
    });

    afterEach(() => {
        clock.uninstall();
    });

    it("uses deferreds that do not resolve at digest", () => {
        var d = $$q.defer();
        d.promise.then(fulfilledSpy);
        $rootScope.$apply(function () { });

        expect(fulfilledSpy).not.toHaveBeenCalled();
    });

    it("uses deferreds that resolve later", () => {
        var d = $$q.defer();
        d.promise.then(fulfilledSpy);
        d.resolve("ok");
        clock.tick(1);

        expect(fulfilledSpy).toHaveBeenCalledWith("ok");
    });

    it("does not invoke digest", () => {
        var d = $$q.defer();
        d.promise.then(_.noop);
        d.resolve("ok");

        var watchSpy = jasmine.createSpy("watchSpy");
        $rootScope.$watch(watchSpy);

        clock.tick(1);

        expect(watchSpy).not.toHaveBeenCalled();
    });
});
