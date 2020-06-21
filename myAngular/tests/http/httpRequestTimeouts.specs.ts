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
    IHttpHeadersGetter,
    IQService
} from "angular";
import { publishExternalAPI } from "../../src/angular_public";

describe("$http", () => {
    var $http: IHttpService;
    var $rootScope: IRootScopeService;
    var $q: IQService;
    var xhr: Sinon.SinonFakeXMLHttpRequest;
    var requests: Sinon.SinonFakeXMLHttpRequest[] = [];
    var url = "http://teropa.info";
    var injector: auto.IInjectorService;
    var clock: jasmine.Clock;

    beforeEach(() => {
        publishExternalAPI();
        injector = createInjector(["ng"]);
        $http = injector.get("$http");
        $rootScope = injector.get("$rootScope");
        $q = injector.get("$q");
        xhr = sinon.useFakeXMLHttpRequest();
        xhr.onCreate = function (req) {
            requests.push(req)
        };
        clock = jasmine.clock();
        clock.install();
    });

    afterEach(() => {
        xhr.restore();
        requests.splice(0, requests.length);
        clock.uninstall();
    });
    
    it("allows aborting a request with a Promise", () => {
        var timeout = $q.defer();
        $http.get(url, {
            timeout: timeout.promise
        });
        $rootScope.$apply();

        timeout.resolve();
        $rootScope.$apply();

        expect(requests[0].aborted).toBe(true);
    });
    
    it("allows aborting a request after a timeout", () => {
        var timeout = $q.defer();
        $http.get(url, {
            timeout: 5000
        });
        $rootScope.$apply();

        clock.tick(5001);

        expect(requests[0].aborted).toBe(true);
    });
});
