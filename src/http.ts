//import * as _ from 'lodash';
import { IHttpBackendService, IQService, IRequestConfig, IRootScopeService } from 'angular';
import * as ain from './angularInterfaces';
import * as _ from "lodash";

export function $HttpProvider() {
    this.$get = ["$httpBackend", "$q", "$rootScope", function ($httpBackend: IHttpBackendService, $q: IQService, $rootScope: IRootScopeService) {
        return function $http(requestConfig: IRequestConfig) {
            var deferred = $q.defer();
            
            var config = _.extend({
                method: "GET"
            }, requestConfig);

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
    }];
}