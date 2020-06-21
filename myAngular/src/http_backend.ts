import { IServiceProviderFactory, IPromise } from "angular";
import * as _ from "lodash";

"use strict";
export function $HttpBackendProvider() {
    this.$get = function () {
        function isPromise(value:any): value is IPromise<any> {
            return value && value.then
        }
        return function (method: string, url: string, post?: any, callback?: Function, headers?: any, timeout?: IPromise<any> | number, withCredentials?: boolean): void {
            var xhr = new XMLHttpRequest();
            var timeoutId;
            xhr.open(method, url, true);
            _.forEach(headers, function (value, key) {
                xhr.setRequestHeader(key, value);
            })
            if (withCredentials) {
                xhr.withCredentials = true;
            }
            xhr.send(post || null);
            xhr.onload = function () {
                if(!_.isUndefined(timeoutId)) {
                    clearTimeout(timeoutId);
                }
                var response = ("response" in xhr) ? xhr.response : xhr.responseText;
                var statusText = xhr.statusText || "";
                callback(xhr.status, response, xhr.getAllResponseHeaders(), statusText);
            };
            xhr.onerror = function () {
                if(!_.isUndefined(timeoutId)) {
                    clearTimeout(timeoutId);
                }
                callback(-1, null, "");
            }
            if (isPromise(timeout)) {
                timeout.then(function () {
                    xhr.abort();
                });
            } else if(timeout > 0) {
                timeoutId = setTimeout(function() {
                    xhr.abort();
                }, timeout);
            }
        }
    };
}

