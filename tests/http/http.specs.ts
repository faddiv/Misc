import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IHttpService, IHttpPromiseCallbackArg, IHttpProvider, auto } from "angular";
import { publishExternalAPI } from "../../src/angular_public";

describe("$http", () => {
    var $http: IHttpService;
    var xhr: Sinon.SinonFakeXMLHttpRequest;
    var requests: Sinon.SinonFakeXMLHttpRequest[] = [];
    var url = "http://teropa.info";
    var injector: auto.IInjectorService;

    beforeEach(() => {
        publishExternalAPI();
        injector = createInjector(["ng"]);
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

    //exposes default headers through provider moved to httpProvider.specs.ts

    it("merges deafult headers case-insensitively", () => {
        $http({
            method: "POST",
            url: url,
            data: "42",
            headers: {
                "content-type": "text/plain;charset=utf-8"
            }
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["content-type"]).toBe("text/plain;charset=utf-8");
        expect(requests[0].requestHeaders["Content-Type"]).toBeUndefined();
    });

    it("does not send content-type header when no data", () => {
        $http({
            method: "POST",
            url: url,
            headers: {
                "content-type": "application/json;charset=utf-8"
            }
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Content-Type"]).not.toBe("application/json;charset=utf-8");
    });

    it("supports functions as header values", () => {
        var contentTypeSpy = jasmine.createSpy("contentTypeSpy").and.returnValue(
            "text/plain;charset=utf-8");
        $http.defaults.headers.post["Content-Type"] = contentTypeSpy;
        $http({
            method: "POST",
            url: url,
            data: 42
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Content-Type"]).toBe("text/plain;charset=utf-8");

    });

    it("ignores header function value when null/undefined", () => {
        var cacheControlSpy = jasmine.createSpy("Cache-Control").and.returnValue(null);
        $http.defaults.headers.post["Cache-Control"] = cacheControlSpy;
        var request = {
            method: "POST",
            url: url,
            data: 42
        };
        $http(request);
        expect(cacheControlSpy).toHaveBeenCalledWith(request);
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Cache-Control"]).toBeUndefined();

    });

    it("makes response headers available", () => {
        var response: IHttpPromiseCallbackArg<any>;
        $http({
            method: "POST",
            url: url,
            data: 42
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, { "Content-Type": "text/plain" }, "Hello");

        expect(response.headers).toBeDefined();
        expect(response.headers instanceof Function).toBe(true);
        expect(response.headers("Content-Type")).toBe("text/plain");
        expect(response.headers("content-type")).toBe("text/plain");
    });

    it("may returns all response headers", () => {
        var response: IHttpPromiseCallbackArg<any>;
        $http({
            method: "POST",
            url: url,
            data: 42
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, { "Content-Type": "text/plain", "Server": "asp-net" }, "Hello");

        expect(response.headers()).toEqual({ "content-type": "text/plain", "server": "asp-net" });
    });

    it("allows setting withCredentials", () => {
        $http({
            method: "POST",
            url: url,
            data: 42,
            withCredentials: true
        });

        expect(requests[0].withCredentials).toBe(true);
    });

    it("allows setting withCredentials from defaults", () => {
        $http.defaults.withCredentials = true;

        $http({
            method: "POST",
            url: url,
            data: 42
        });

        expect(requests[0].withCredentials).toBe(true);
    });

    it("allows transforming requests with functions", () => {
        $http({
            method: "POST",
            url: url,
            data: 42,
            transformRequest(data) {
                return "*" + data + "*";
            }
        });

        expect(requests[0].requestBody).toBe("*42*");
    });

    it("allows multiple transform functions", () => {
        $http({
            method: "POST",
            url: url,
            data: 42,
            transformRequest: [
                function (data) {
                    return "*" + data + "*";
                }, function (data) {
                    return "-" + data + "-";
                }
            ]
        });

        expect(requests[0].requestBody).toBe("-*42*-");
    });

    it("allows settings transforms in defaults", () => {
        $http.defaults.transformRequest = [
            function (data) {
                return "*" + data + "*";
            }, function (data) {
                return "-" + data + "-";
            }
        ];

        $http({
            method: "POST",
            url: url,
            data: 42,
        });

        expect(requests[0].requestBody).toBe("-*42*-");
    });

    it("passes request headers getter to transforms", () => {
        $http.defaults.transformRequest = function (data, headers) {
            if (headers("Content-Type") === "text/emphasized") {
                return "*" + data + "*";
            } else {
                return data;
            }
        };
        $http({
            method: "POST",
            url: url,
            data: 42,
            headers: {
                "content-type": "text/emphasized"
            }
        });

        expect(requests[0].requestBody).toBe("*42*");
    });

    it("allows transforming responses with functions", () => {
        var response;
        $http({
            method: "POST",
            url: url,
            data: 42,
            transformResponse(data) {
                return "*" + data + "*";
            }
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, { "Content-Type": "text/plain" }, "Hello");

        expect(response.data).toEqual("*Hello*");
    });

    it("passes response headers to transform functions", () => {
        var response;
        $http({
            method: "POST",
            url: url,
            data: 42,
            transformResponse(data, headers) {
                if (headers("content-type") === "text/decorated") {
                    return "*" + data + "*";
                } else {
                    return data;
                }
            }
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, { "Content-Type": "text/decorated" }, "Hello");

        expect(response.data).toEqual("*Hello*");
    });

    it("allows setting default response transforms", () => {
        $http.defaults.transformResponse = [function (data) {
            return "*" + data + "*";
        }];
        var response;
        $http({
            method: "POST",
            url: url,
            data: 42
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, { "Content-Type": "text/plain" }, "Hello");

        expect(response.data).toEqual("*Hello*");
    });

    it("transforms error responses also", () => {
        var response;
        $http({
            method: "POST",
            url: url,
            data: 42,
            transformResponse(data) {
                return "*" + data + "*";
            }
        }).catch(r => {
            response = r;
        });

        requests[0].respond(401, { "Content-Type": "text/plain" }, "Fail");

        expect(response.data).toEqual("*Fail*");
    });

    it("passes HTTP status to response transformers", () => {
        var response;
        $http({
            url: url,
            transformResponse(data, headers, status) {
                if (status === 401) {
                    return "unauthorized";
                } else {
                    return data;
                }
            }
        }).catch(r => {
            response = r;
        });

        requests[0].respond(401, { "Content-Type": "text/plain" }, "Fail");

        expect(response.data).toEqual("unauthorized");
    });

    it("serializes object data to JSON for requests", () => {
        $http({
            method: "POST",
            url: url,
            data: { aKey: 42 }
        });

        expect(requests[0].requestBody).toBe('{"aKey":42}');
    });

    it("serializes array data to JSON for requests", () => {
        $http({
            method: "POST",
            url: url,
            data: [1, "two", 3]
        });

        expect(requests[0].requestBody).toBe('[1,"two",3]');
    });

    it("does not serialize blobs for requests", () => {
        var blob;
        if (window.Blob) {
            blob = new Blob(["hello"]);
        } else {
            var BlobBuilder = window["BlobBuilder"] || window["WebKitBlobBuilder"] ||
                window["MozBlobBuilder"] || window["MSBlobBuilder"];
            var bb = new BlobBuilder();
            bb.append("hello");
            blob = bb.getBlob("text/plain");
        }
        $http({
            method: "POST",
            url: url,
            data: blob
        });

        expect(requests[0].requestBody).toBe(blob);
    });

    it("does not serialize form data for requests", () => {
        var formData = new FormData();
        formData.append("aField", "aValue");

        $http({
            method: "POST",
            url: url,
            data: formData
        });

        expect(requests[0].requestBody).toBe(formData);
    });

    it("parses JSON data for JSON responses", () => {
        var response;

        $http({
            method: "GET",
            url: url,
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, { "Content-Type": "application/json" }, '{"message":"hello"}');
        expect(_.isObject(response.data)).toBe(true);
        expect(response.data).toEqual({ message: "hello" });
    });

    it("parses JSON object response without content type", () => {
        var response;

        $http({
            method: "GET",
            url: url,
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, {}, '{"message":"hello"}');
        expect(_.isObject(response.data)).toBe(true);
        expect(response.data).toEqual({ message: "hello" });
    });

    it("parses JSON array response without content type", () => {
        var response;

        $http({
            method: "GET",
            url: url,
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, {}, '[1,"2",3]');
        expect(_.isArray(response.data)).toBe(true);
        expect(response.data).toEqual([1, "2", 3]);
    });

    it("does not choke on response resembling JSON but not valid", () => {
        var response;

        $http({
            method: "GET",
            url: url,
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, {}, '{1,"2",3]');
        expect(response.data).toEqual('{1,"2",3]');
    });

    it("does not try to parse interpolation expr as JSON", () => {
        var response;

        $http({
            method: "GET",
            url: url,
        }).then(r => {
            response = r;
        });

        requests[0].respond(200, {}, "{{expr}}");
        expect(response.data).toEqual("{{expr}}");
    });

    it("adds params to URL", () => {
        $http({
            url: url,
            params: {
                a: 42,
                b: 43
            }
        });

        expect(requests[0].url).toBe(url + "?a=42&b=43");
    });

    it("adds additional params to URL", () => {
        $http({
            url: url + "?a=42",
            params: {
                b: 43
            }
        });

        expect(requests[0].url).toBe(url + "?a=42&b=43");
    });

    it("escapes url characters in params", () => {
        $http({
            url: url,
            params: {
                "==": "&&"
            }
        });

        expect(requests[0].url).toBe(url + "?%3D%3D=%26%26");
    });

    it("does not attach null or undefined params", () => {
        $http({
            url: url,
            params: {
                a: null,
                b: undefined
            }
        });

        expect(requests[0].url).toBe(url);
    });

    it("attaches multiple params for arrays", () => {
        $http({
            url: url,
            params: {
                a: [42, 43]
            }
        });

        expect(requests[0].url).toBe(url + "?a=42&a=43");
    });

    it("serializes objects to json", () => {
        $http({
            url: url,
            params: {
                a: { b: 42 }
            }
        });

        expect(requests[0].url).toBe(url + "?a=%7B%22b%22%3A42%7D");
    });

    it("serializes dates to ISO strings", () => {
        $http({
            url: url,
            params: {
                a: new Date(2015, 0, 1, 12, 0, 0, 0)
            }
        });
        var $rootScope = injector.get("$rootScope");
        $rootScope.$apply();

        expect(/\d{4}-\d{2}-\d{2}T\d{2}%3A\d{2}%3A\d{2}/.test(requests[0].url)).toBeTruthy();
    });

    it("allows substituting param serializer", () => {
        $http({
            url: url,
            params: {
                a: 42,
                b: 43
            },
            paramSerializer(params: any): string {
                return _.map(params, (v, k) => {
                    return k + "=" + v + "lol";
                }).join("&");
            }
        });

        expect(requests[0].url).toBe(url + "?a=42lol&b=43lol");
    });

});
