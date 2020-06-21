import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import {
    auto, IInterpolateProvider, IInterpolateService
} from "angular";
import { publishExternalAPI } from "../../src/angular_public";
import { IInterpolationFunctionInternal } from "../../src/angularInterfaces";

describe("$interpolate", () => {
    var injector: auto.IInjectorService;
    var $interpolate: IInterpolateService;
    beforeEach(() => {
        publishExternalAPI();
        injector = createInjector(["ng"]);
        $interpolate = injector.get("$interpolate");
    });

    afterEach(() => {
        delete window.angular;
    });

    //Optimizing Interpolation Watches With A Watch Delegate
    it("uses a watch delegate", () => {
        var interp = <IInterpolationFunctionInternal>$interpolate("has an {{expr}}");
        expect(interp.$$watchDelegate).toBeDefined();
    });

    it("correctly returns new and old value when watched", () => {
        var $rootScope = injector.get("$rootScope");
        var interp = <IInterpolationFunctionInternal>$interpolate("{{expr}}");
        var listenerSpy = jasmine.createSpy("listenerSpy");
        $rootScope.$watch(interp, listenerSpy);
        $rootScope.expr = 42;
        $rootScope.$apply();
        var call = listenerSpy.calls.mostRecent();
        expect(call.args[0]).toEqual("42");
        expect(call.args[1]).toEqual("42");

        $rootScope.expr++;
        $rootScope.$apply();
        call = listenerSpy.calls.mostRecent();
        expect(call.args[0]).toEqual("43");
        expect(call.args[1]).toEqual("42");
        expect(call.args[2]).toEqual($rootScope);
    });
});