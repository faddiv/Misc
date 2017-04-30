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
        injector = createInjector(["ng"]);
        $http = injector.get("$http");
        $rootScope = injector.get("$rootScope");
        xhr = sinon.useFakeXMLHttpRequest();
        xhr.onCreate = function (req) {
            requests.push(req)
        }
    });

    afterEach(() => {
        xhr.restore();
        requests.splice(0, requests.length);
    });

    describe('pending requests', function () {
        it("are in the collection while pending", () => {
            $http.get(url);
            $rootScope.$apply();

            expect($http.pendingRequests).toBeDefined();
            expect($http.pendingRequests.length).toBe(1);
            expect($http.pendingRequests[0].url).toBe(url);

            requests[0].respond(200, {}, "OK");
            $rootScope.$apply();

            expect($http.pendingRequests.length).toBe(0);
        });

        it("are also cleared on failure", () => {
            $http.get(url);
            $rootScope.$apply();

            requests[0].respond(404, {}, "Not found");
            $rootScope.$apply();

            expect($http.pendingRequests.length).toBe(0);
        });
    });
});
