import { IServiceProviderFactory } from "angular";

"use strict";
export function $HttpBackendProvider() {
    this.$get = function () {
        return function (method: string, url: string, post?: any, callback?: Function, headers?: any, timeout?: number, withCredentials?: boolean): void {
            var xhr = new XMLHttpRequest();
            xhr.open(method, url, true);
            xhr.send(post || null);
            xhr.onload = function () {
                var response = ("response" in xhr) ? xhr.response : xhr.responseText;
                var statusText = xhr.statusText || "";
                callback(xhr.status, response, statusText);
            };
            xhr.onerror = function() { 
                callback(-1, null, "");
             }
        }
    };
}

