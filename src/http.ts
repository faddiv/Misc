//import * as _ from 'lodash';
import { IHttpBackendService, IQService, IRequestConfig, IRootScopeService, IHttpProviderDefaults, IHttpService, IHttpRequestConfigHeaders } from 'angular';
import * as ain from './angularInterfaces';
import * as _ from "lodash";

export function $HttpProvider() {
    var defaults: IHttpProviderDefaults = this.defaults = {
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

    function mergeHeaders(config: IRequestConfig): IHttpRequestConfigHeaders {
        var reqHeaders: IHttpRequestConfigHeaders = _.extend(
            {},
            config.headers
        );
        var defHeaders = _.extend(
            {},
            defaults.headers.common,
            defaults.headers[(config.method || "get").toLowerCase()]
        );
        _.forEach(defHeaders, function (value, key) {
            var headerExists = _.some(reqHeaders, function (v, k) {
                return k.toLowerCase() === key.toLowerCase();
            });
            if (!headerExists) {
                reqHeaders[key] = value;
            }
        });
        return executeHeaderFns(reqHeaders, config);
    }
    function executeHeaderFns(headers: IHttpRequestConfigHeaders, config): IHttpRequestConfigHeaders {
        return _.transform(headers, function (result, v, k) {
            if (_.isFunction(v)) {
                var returnValue = v(config);
                if (_.isNull(returnValue) || _.isUndefined(returnValue)) {
                    delete result[k];
                } else {
                    result[k] = returnValue;
                }
            }
        }, headers);
    }
    this.$get = ["$httpBackend", "$q", "$rootScope", function ($httpBackend: IHttpBackendService, $q: IQService, $rootScope: IRootScopeService) {
        function $http(requestConfig: IRequestConfig) {
            var deferred = $q.defer();

            var config = _.extend({
                method: "GET",
            }, requestConfig);
            config.headers = mergeHeaders(requestConfig)

            if (_.isUndefined(config.data)) {
                _.forEach(config.headers, function (v, k) {
                    if (k.toLowerCase() === "content-type") {
                        delete config.headers[k];
                    }
                });
            }
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