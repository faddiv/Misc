//import * as _ from 'lodash';
import {
    IHttpBackendService,
    IQService,
    IRequestConfig,
    IRootScopeService,
    IHttpProviderDefaults,
    IHttpService,
    IHttpRequestConfigHeaders,
    IHttpRequestTransformer,
    IHttpResponseTransformer
} from 'angular';
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
        },
        transformRequest: [
            function (data) {
                if (_.isObject(data) && !isBlob(data) && !isFile(data) && !isFormData(data)) {
                    return JSON.stringify(data);
                } else {
                    return data;
                }
            }
        ],
        transformResponse: [defaultHttpResponseTransform]
    };

    function isBlob(data: Object) {
        return data.toString() === "[object Blob]";
    }

    function isFile(data) {
        return data.toString() === "[object File]";
    }

    function isFormData(data) {
        return data.toString() === "[object FormData]";
    }

    function defaultHttpResponseTransform(data, headers) {
        if (_.isString(data)) {
            var contentType = headers("Content-Type");
            if ((contentType && contentType.indexOf("application/json") === 0)
                || isJsonLike(data)) {
                return JSON.parse(data);
            }
        }
        return data;
    }

    function isJsonLike(data: string) {
        if (data.match(/^\{(?!\{)/)) {
            return data.match(/\}$/);
        } else if (data.match(/^\[/)) {
            return data.match(/\]$/);
        }
    }

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
    function isSuccess(status: number) {
        return 200 <= status && status < 300;
    }

    function parseHeaders(headers: string | any) {
        if (_.isString(headers)) {
            var lines = headers.split("\n");
            return _.transform(lines, (result, line: string) => {
                var separatorAt = line.indexOf(":");
                var name = _.trim(line.substr(0, separatorAt)).toLowerCase();
                var value = _.trim(line.substr(separatorAt + 1));
                if (name) {
                    result[name] = value;
                }
            }, {});
        } else {
            return _.transform(headers, function (result, v, k: string) {
                result[_.trim(k.toLowerCase())] = _.trim(v);
            }, {});
        }
    }
    function headersGetter(headers: string) {
        var headersObj: any;
        return function (name: string) {
            headersObj = headersObj || parseHeaders(headers);
            if (name) {
                return headersObj[name.toLowerCase()];
            } else {
                return headersObj;
            }
        }
    }

    function transformData(data: any, headers: any, status: number, transformResponse: IHttpResponseTransformer | IHttpResponseTransformer[]);
    function transformData(data: any, headers: any, status: number, transformRequest: IHttpRequestTransformer | IHttpRequestTransformer[]);
    function transformData(data: any, headers: any, status: number, transformRequest: Function | Function[]) {
        if (_.isUndefined(transformRequest)) {
            return data;
        }
        if (_.isFunction(transformRequest)) {
            return transformRequest(data, headers, status);
        } else {
            return _.reduce(transformRequest, (data, fn) => {
                return fn(data, headers, status);
            }, data);
        }
    }



    this.$get = ["$httpBackend", "$q", "$rootScope", function ($httpBackend: IHttpBackendService, $q: IQService, $rootScope: IRootScopeService) {

        function sendReq(config, reqData) {
            var deferred = $q.defer();
            function done(status: number, response: any, headersString: string, statusText: string) {
                status = Math.max(status, 0);
                var finishMethod = isSuccess(status) ? "resolve" : "reject";
                deferred[finishMethod]({
                    status: status,
                    data: response,
                    statusText: statusText,
                    headers: headersGetter(headersString),
                    config: config
                });
                if (!$rootScope.$$phase) {
                    $rootScope.$apply();
                }
            }
            $httpBackend(
                config.method,
                config.url,
                reqData,
                done,
                config.headers,
                undefined,
                config.withCredentials);
            return deferred.promise;
        }

        function $http(requestConfig: IRequestConfig) {
            var config = _.extend({
                method: "GET",
                transformRequest: defaults.transformRequest,
                transformResponse: defaults.transformResponse
            }, requestConfig);
            config.headers = mergeHeaders(requestConfig)

            if (_.isUndefined(config.withCredentials) &&
                !_.isUndefined(defaults.withCredentials)) {
                config.withCredentials = defaults.withCredentials;
            }
            var reqData = transformData(
                config.data,
                headersGetter(config.headers),
                undefined,
                config.transformRequest);

            if (_.isUndefined(reqData)) {
                _.forEach(config.headers, function (v, k) {
                    if (k.toLowerCase() === "content-type") {
                        delete config.headers[k];
                    }
                });
            }

            function transformResponse(response) {
                if (response.data) {
                    response.data = transformData(
                        response.data,
                        response.headers,
                        response.status
                        config.transformResponse);
                }
                if (isSuccess(response.status)) {
                    return response;
                } else {
                    return $q.reject(response);
                }
            }

            return sendReq(config, reqData)
                .then(transformResponse, transformResponse);
        }
        (<IHttpService>$http).defaults = defaults;
        return $http;
    }];
}