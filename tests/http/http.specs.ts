import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IHttpService, IHttpPromiseCallbackArg } from "angular";
import { publishExternalAPI } from "../../src/angular_public";

describe("setupModuleLoader", () => {
    var $http: IHttpService;
    var xhr: Sinon.SinonFakeXMLHttpRequest;
    var requests: Sinon.SinonFakeXMLHttpRequest[] = [];
    var url = "http://teropa.info";

    beforeEach(() => {
        publishExternalAPI();
        var injector = createInjector(["ng"]);
        $http = injector.get("$http");
        xhr = sinon.useFakeXMLHttpRequest();
        xhr.onCreate = function (req) {
            requests.push(req)
        }
    });

    afterEach(() => {
        xhr.restore();
        requests.splice(0, requests.length);
    });

    it("is a Function", () => {
        expect($http instanceof Function).toBe(true);
    });

    it("returns a Promise", () => {
        var result = $http({ method: "get", url: "/" });
        expect(result).toBeDefined();
        expect(result.then).toBeDefined();
    });

    it("makes an XMLHttpRequest to given url", () => {
        $http({
            method: "POST",
            url: url,
            data: "hello"
        });

        expect(requests.length).toBe(1);
        var request = requests[0];
        expect(request.method).toBe("POST");
        expect(request.url).toBe(url);
        expect(request.async).toBe(true);
        expect(request.requestBody).toBe("hello")
    });

    it("resolves promise when XHR result received", () => {
        var response: IHttpPromiseCallbackArg<any>;
        $http({
            method: "GET",
            url: url
        }).then<any>(function (r) { response = r });

        requests[0].respond(200, {}, "Hello");

        expect(response).toBeDefined();
        expect(response.status).toBe(200);
        expect(response.statusText).toBe("OK");
        expect(response.data).toBe("Hello");
        expect(response.config.url).toEqual(url);
    });

    it("rejects promise when XHR result received with error status", () => {
        var response: IHttpPromiseCallbackArg<any>;
        $http({
            method: "GET",
            url: url
        }).catch<any>(function (r) { response = r });

        requests[0].respond(401, {}, "Fail");

        expect(response).toBeDefined();
        expect(response.status).toBe(401);
        expect(response.statusText).toBe("Unauthorized");
        expect(response.data).toBe("Fail");
        expect(response.config.url).toEqual(url);
    });

    it("rejects promise when XHR result errors/aborts", () => {
        var response: IHttpPromiseCallbackArg<any>;
        $http({
            method: "GET",
            url: url
        }).catch<any>(function (r) { response = r });

        requests[0]["onerror"]();

        expect(response).toBeDefined();
        expect(response.status).toBe(0);
        expect(response.data).toBe(null);
        expect(response.config.url).toEqual(url);
    });

    it("uses GET method by default", () => {
        $http({
            url: url
        });
        expect(requests.length).toBe(1);
        expect(requests[0].method).toBe("GET");
    });

    it("sets headers on request", () => {
        $http({
            url: url,
            headers: {
                "Accept": "text/plain",
                "Cache-Control": "no-cache"
            }
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Accept"]).toBe("text/plain");
        expect(requests[0].requestHeaders["Cache-Control"]).toBe("no-cache");
    });

    it("sets default headers on request", () => {
        $http({
            url: url
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Accept"]).toBe("applictaion/json, text/plain, */*");
    });

    _.forEach(["POST", "PUT", "PATCH"], method => {
        it("sets method-specific default headers on request " + method, () => {
            $http({
                url: url,
                method: method,
                data: 42
            });
            expect(requests.length).toBe(1);
            expect(requests[0].requestHeaders["Content-Type"]).toBe("application/json;charset=utf-8");
        });
    });
    //Additional test
    it("do not sets Content-Type for GET", () => {
        $http({
            url: url,
            method: "GET"
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Content-Type"]).toBe("text/plain;charset=utf-8");
    });

    it("exposes default headers for overriding", () => {
        $http.defaults.headers.post["Content-Type"] = "text/plain;charset=utf-8";
        $http({
            url: url,
            method: "POST",
            data: 42
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Content-Type"]).toBe("text/plain;charset=utf-8");
    });
});
