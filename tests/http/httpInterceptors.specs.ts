import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IHttpService, IHttpPromiseCallbackArg, IHttpProvider, auto } from "angular";
import { publishExternalAPI } from "../../src/angular_public";

describe("setupModuleLoader", () => {
    var $http: IHttpService;
    var xhr: Sinon.SinonFakeXMLHttpRequest;
    var requests: Sinon.SinonFakeXMLHttpRequest[] = [];
    var url = "http://teropa.info";

    beforeEach(() => {
        publishExternalAPI();
        xhr = sinon.useFakeXMLHttpRequest();
        xhr.onCreate = function (req) {
            requests.push(req)
        }
    });

    it("allows attaching interceptor factories", () => {
        var interceptorFactorySpy = jasmine.createSpy("interceptorFactorySpy");
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.interceptors.push(interceptorFactorySpy);
        }]);

        $http = injector.get("$http");

        expect(interceptorFactorySpy).toHaveBeenCalled();
    });

    it("uses DI to instantiate interceptors", () => {
        var interceptorFactorySpy = jasmine.createSpy("interceptorFactorySpy");
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.interceptors.push(["$rootScope", interceptorFactorySpy]);
        }]);

        $http = injector.get("$http");
        var $rootScope = injector.get("$rootScope");

        expect(interceptorFactorySpy).toHaveBeenCalledWith($rootScope);
    });
});