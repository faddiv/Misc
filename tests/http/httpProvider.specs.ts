import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import { IHttpService, IHttpPromiseCallbackArg, IHttpProvider } from "angular";
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
    it("exposes default headers through provider", () => {
        var injector = createInjector(["ng", function ($httpProvider: IHttpProvider) {
            $httpProvider.defaults.headers.post["Content-Type"] = "text/plain;charset=utf-8";
        }]);
        $http = injector.get("$http");
        $http({
            url: url,
            method: "POST",
            data: 42
        });
        expect(requests.length).toBe(1);
        expect(requests[0].requestHeaders["Content-Type"]).toBe("text/plain;charset=utf-8");
    });
});
