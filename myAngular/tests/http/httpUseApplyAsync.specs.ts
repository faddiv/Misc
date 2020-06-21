import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import {
    IHttpService,
    IHttpPromiseCallbackArg,
    IHttpProvider,
    IHttpParamSerializer,
    IRootScopeService,
    auto,
    IHttpHeadersGetter
} from "angular";
import { publishExternalAPI } from "../../src/angular_public";

describe("$http", () => {
    var $http: IHttpService;
    var $rootScope: IRootScopeService;
    var xhr: Sinon.SinonFakeXMLHttpRequest;
    var requests: Sinon.SinonFakeXMLHttpRequest[] = [];
    var url = "http://teropa.info";
    var injector: auto.IInjectorService;

    beforeEach(() => {
        publishExternalAPI();
        xhr = sinon.useFakeXMLHttpRequest();
        xhr.onCreate = function (req) {
            requests.push(req)
        }
    });

    afterEach(() => {
        xhr.restore();
        requests.splice(0, requests.length);
    });

    describe("useApplyAsync", () => {

        var clock: jasmine.Clock;
        beforeEach(() => {
            injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
                $httpProvider.useApplyAsync(true);
            }]);
            $http = injector.get("$http");
            $rootScope = injector.get("$rootScope");
            clock = jasmine.clock();
            clock.install();
        });

        afterEach(() => {
            clock.uninstall();
        });

        it("does not resolve promise immediately when enabled", () => {
            var resolveSpy = jasmine.createSpy("resolveSpy");
            $http.get(url).then(resolveSpy);
            $rootScope.$apply();
            requests[0].respond(200, {}, "OK");
            expect(resolveSpy).not.toHaveBeenCalled();
        });

        it("resolves promise later when enabled", () => {
            var resolveSpy = jasmine.createSpy("resolveSpy");
            $http.get(url).then(resolveSpy);
            $rootScope.$apply();
            requests[0].respond(200, {}, "OK");
            clock.tick(100);
            expect(resolveSpy).toHaveBeenCalled();
        });
    });
});
