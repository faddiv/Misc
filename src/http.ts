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
    IHttpResponseTransformer,
    auto
} from 'angular';
import * as ain from './angularInterfaces';
import * as _ from "lodash";

interface ISanitizedRequestConfig extends IRequestConfig {
    paramSerializer?: (obj: any) => string;
}

export function $HttpParamSerializerProvider() {

    this.$get = function () {
        function serializeParams(params: any): string {
            var parts = [];
            _.forEach(params, (value, key) => {
                if (_.isUndefined(value) || _.isNull(value)) {
                    return;
                }
                if (!_.isArray<string>(value)) {
                    value = [value];
                }
                _.forEach(value, function (v) {
                    if (_.isObject(v)) {
                        v = JSON.stringify(v);
                    }
                    parts.push(encodeURIComponent(key) + "=" + encodeURIComponent(v));
                })
            });
            return parts.join("&");
        }
        return serializeParams;
    }
}

export function $HttpParamSerializerJQLikeProvider() {
    this.$get = function () {
        function serializeParams(params: any): string {
            var parts = [];

            function serialize(value, prefix: string, topLevel?: boolean) {
                if (_.isUndefined(value) || _.isNull(value)) {
                    return;
                }
                if (_.isArray(value)) {
                    _.forEach(value, function (v, i) {
                        serialize(v, prefix + "[" +
                            (_.isObject(v) ? i : "") +
                            "]");
                    });
                } else if (_.isObject(value) && !_.isDate(value)) {
                    _.forEach(value, function (v, k) {
                        var pre = prefix +
                            (topLevel ? "" : "[") +
                            k +
                            (topLevel ? "" : "]");
                        serialize(v, pre);
                    });
                } else {
                    parts.push(encodeURIComponent(prefix) + "=" + encodeURIComponent(value));
                }
            }
            serialize(params, "", true);
            return parts.join("&");
        }
        return serializeParams;
    }
}

export function $HttpProvider() {
    var interceptorFactories = this.interceptors = [];
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
        transformResponse: [defaultHttpResponseTransform],
        paramSerializer: "$httpParamSerializer"
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
            return _.transform(headers, function (result, v: string, k: string) {
                result[_.trim(k.toLowerCase())] = _.trim(v);
            }, {});
        }
    }

    function headersGetter(headers: string | any) {
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

    function buildUrl(url: string, params: string): string {
        if (params.length) {
            url += (url.indexOf("?") === -1) ? "?" : "&";
            url += params;
        }
        return url;
    }

    this.$get = ["$httpBackend", "$q", "$rootScope", "$injector", function ($httpBackend: IHttpBackendService, $q: IQService, $rootScope: IRootScopeService, $injector: auto.IInjectorService) {

        var interceptors = _.map(interceptorFactories, function(fn: any) { 
            return $injector.invoke(fn);
         })

        function sendReq(config: ISanitizedRequestConfig, reqData) {
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
            var url = buildUrl(config.url, config.paramSerializer(config.params));
            $httpBackend(
                config.method,
                url,
                reqData,
                done,
                config.headers,
                undefined,
                config.withCredentials);
            return deferred.promise;
        }

        function $http(requestConfig: IRequestConfig) {
            var config: IRequestConfig = _.extend({
                method: "GET",
                transformRequest: defaults.transformRequest,
                transformResponse: defaults.transformResponse,
                paramSerializer: defaults.paramSerializer
            }, requestConfig);
            config.headers = mergeHeaders(requestConfig);
            if (_.isString(config.paramSerializer)) {
                config.paramSerializer = $injector.get<(obj: any) => string>(config.paramSerializer);
            }

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
                        response.status,
                        config.transformResponse);
                }
                if (isSuccess(response.status)) {
                    return response;
                } else {
                    return $q.reject(response);
                }
            }

            return sendReq(<any>config, reqData)
                .then(transformResponse, transformResponse);
        }
        (<IHttpService>$http).defaults = defaults;

        _.forEach(["get", "head", "delete"], function (method: string) {
            $http[method] = function (url: string, config: IRequestConfig) {
                return $http(_.extend(config || {}, {
                    method: method.toUpperCase(),
                    url: url
                }));
            };
        });

        _.forEach(["post", "put", "patch"], function (method: string) {
            $http[method] = function (url: string, data: any, config: IRequestConfig) {
                return $http(_.extend(config || {}, {
                    method: method.toUpperCase(),
                    url: url,
                    data: data
                }));
            };
        });
        return $http;
    }];
}