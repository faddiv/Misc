//import * as _ from 'lodash';
import { IHttpBackendService, IQService, IRequestConfig, IRootScopeService, IHttpProviderDefaults, IHttpService } from 'angular';
import * as ain from './angularInterfaces';
import * as _ from "lodash";

export function $HttpProvider() {
    var defaults: IHttpProviderDefaults = {
        headers: {
            common: {
                Accept: "applictaion/json, text/plain, */*"
            },
            post: {
                "Content-Type": "application/json;charset=utf-8"
            },
            put: {
                "Content-Type": "application/json;charset=utf-8"
            },
            patch: {
                "Content-Type": "application/json;charset=utf-8"
            }
        }
    };

    function mergeHeaders(config: IRequestConfig) {
        return _.extend(
            {},
            defaults.headers.common,
            defaults.headers[(config.method || "get").toLowerCase()],
            config.headers
        );
    }
    this.$get = ["$httpBackend", "$q", "$rootScope", function ($httpBackend: IHttpBackendService, $q: IQService, $rootScope: IRootScopeService) {
        function $http(requestConfig: IRequestConfig) {
            var deferred = $q.defer();

            var config = _.extend({
                method: "GET",
            }, requestConfig);
            config.headers = mergeHeaders(requestConfig)

            function isSuccess(status: number) {
                return 200 <= status && status < 300;
            }

            function done(status: number, response: any, statusText: string) {
                status = Math.max(status, 0);
                var finishMethod = isSuccess(status) ? "resolve" : "reject";
                deferred[finishMethod]({
                    status: status,
                    data: response,
                    statusText: statusText,
                    config: config
                });
                if (!$rootScope.$$phase) {
                    $rootScope.$apply();
                }
            }
            $httpBackend(config.method, config.url, config.data, done, config.headers);
            return deferred.promise;
        }
        (<IHttpService>$http).defaults = defaults;
        return $http;
    }];
}