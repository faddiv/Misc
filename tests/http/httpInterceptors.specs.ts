import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IHttpService, IHttpPromiseCallbackArg, IHttpProvider, auto, IQService } from "angular";
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

    afterEach(() => {
        xhr.restore();
        requests.splice(0, requests.length);
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

    it("allows referencing existing interceptor factories", () => {

        var interceptorFactorySpy = jasmine.createSpy("interceptorFactorySpy").and.returnValue({});
        var injector = createInjector(["ng", function ($provide: auto.IProvideService, $httpProvider: IHttpProvider) {
            $provide.factory("myInterceptor", interceptorFactorySpy);
            $httpProvider.interceptors.push("myInterceptor");
        }]);

        $http = injector.get("$http");

        expect(interceptorFactorySpy).toHaveBeenCalled();
    });

    it("allows intercepting requests", () => {
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.interceptors.push(function () {
                return {
                    request: function (config) {
                        config.params.intercepted = true;
                        return config;
                    }
                };
            });
        }]);
        $http = injector.get("$http");
        var $rootScope = injector.get("$rootScope");

        $http.get(url, { params: {} });
        $rootScope.$apply();
        expect(requests[0].url).toBe(url + "?intercepted=true");
    });

    it("allows returning promises from request intercepts", () => {
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.interceptors.push(function ($q: IQService) {
                return {
                    request: function (config) {
                        config.params.intercepted = true;
                        return $q.when(config);
                    }
                };
            });
        }]);
        $http = injector.get("$http");
        var $rootScope = injector.get("$rootScope");

        $http.get(url, { params: {} });
        $rootScope.$apply();
        expect(requests[0].url).toBe(url + "?intercepted=true");
    });

    it("allows intercepting responses", () => {
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.interceptors.push(function () {
                return {
                    response: function (response) {
                        response.intercepted = true;
                        return response;
                    }
                };
            });
        }]);
        $http = injector.get("$http");
        var $rootScope = injector.get("$rootScope");

        var response;
        $http.get(url).then(r => {
            response = r;
        });
        $rootScope.$apply();
        requests[0].respond(200, {}, "Hello");
        expect(response.intercepted).toBe(true);
    });

    it("allows intercepting request errors", () => {
        var requestErrorSpy = jasmine.createSpy("requestErrorSpy");
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.interceptors.push(function () {
                return {
                    request: function () {
                        throw "fail";
                    }
                };
            });
            $httpProvider.interceptors.push(function () {
                return {
                    requestError: requestErrorSpy
                };
            });
        }]);
        $http = injector.get("$http");
        var $rootScope = injector.get("$rootScope");

        $http.get(url);
        $rootScope.$apply();

        expect(requests.length).toBe(0);
        expect(requestErrorSpy).toHaveBeenCalledWith("fail");
    });

    it("allows intercepting response errors", () => {
        var responseErrorSpy = jasmine.createSpy("requestErrorSpy");
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.interceptors.push(function () {
                return {
                    responseError: responseErrorSpy
                };
            });
            $httpProvider.interceptors.push(function () {
                return {
                    response: function () {
                        throw "fail";
                    }
                };
            });
        }]);
        $http = injector.get("$http");
        var $rootScope = injector.get("$rootScope");

        $http.get(url);
        $rootScope.$apply();

        requests[0].respond(200, {}, "hello");
        $rootScope.$apply();
        
        expect(responseErrorSpy).toHaveBeenCalledWith("fail");
    });
});