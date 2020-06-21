import * as _ from 'lodash';
import { IQProvider, IQResolveReject, IPromise, IDeferred, IRootScopeService, IQService } from "angular";
import { IPromiseState } from "./angularInterfaces";
"use strict";
enum QStatus {
    Resolved = 1,
    Rejected = 2,
    Notify = 3
}
function qFactory(callLater: (callback: Function) => void): IQService {

    function processQueue(state: IPromiseState) {
        var pending = state.pending;
        state.pending = undefined;
        _.forEach(pending, (handlers) => {
            var deferred = handlers[0];
            var fn = handlers[state.status];
            try {
                if (_.isFunction(fn)) {
                    deferred.resolve(fn(state.value));
                } else if (state.status === QStatus.Resolved) {
                    deferred.resolve(state.value);
                } else {
                    deferred.reject(state.value);
                }
            } catch (e) {
                deferred.reject(e);
            }
        });
    }

    function scheduleProcessQueue(state: IPromiseState) {
        callLater(function () {
            processQueue(state);
        });
    }

    function isPromise<T>(value: any): value is IPromise<T> {
        return value && _.isFunction(value.then);
    }

    function makePromise(value: any, resolved: boolean) {
        var d = new Deferred<any>();
        if (resolved) {
            d.resolve(value);
        } else {
            d.reject(value);
        }
        return d.promise;
    }

    function handleFinallyCallback(callback: any, value: any, resolved: boolean) {
        var callbackValue = callback();
        if (isPromise(callbackValue)) {
            return callbackValue.then(function () {
                return makePromise(value, resolved);
            });
        } else {
            return makePromise(value, resolved);
        }
    }

    class Promise<T> implements IPromise<T> {
        $$state: IPromiseState = {};
        constructor() {
        }

        then<TResult>(onFulfilled: (promiseValue: T) => TResult | IPromise<TResult>, onRejected?: (reason: any) => any, notifyCallback?: (state: any) => any): IPromise<TResult> {
            var result = new Deferred();
            this.$$state.pending = this.$$state.pending || [];
            this.$$state.pending.push([result, onFulfilled, onRejected, notifyCallback]);
            if (this.$$state.status > 0) {
                scheduleProcessQueue(this.$$state);
            }
            return result.promise;
        }

        catch<TResult>(onRejected: (reason: any) => TResult | IPromise<TResult>): IPromise<TResult> {
            return this.then(null, onRejected);
        }

        finally(finallyCallback: () => any, notifyCallback?: (state: any) => any): IPromise<T> {
            return this.then<any>(function (value) {
                return handleFinallyCallback(finallyCallback, value, true);
            }, function (rejection) {
                return handleFinallyCallback(finallyCallback, rejection, false);
            }, notifyCallback);
        }

    }

    class Deferred<T> implements IDeferred<T> {
        constructor() {
            this.promise = new Promise<T>();
        }

        resolve(value?: T | IPromise<T>): void {
            if (this.promise.$$state.status) {
                return;
            }
            if (isPromise(value)) {
                value.then(
                    <any>_.bind(this.resolve, this),
                    <any>_.bind(this.reject, this),
                    <any>_.bind(this.notify, this)
                );
            } else {
                this.promise.$$state.value = value;
                this.promise.$$state.status = QStatus.Resolved;
                scheduleProcessQueue(this.promise.$$state);
            }
        }

        reject(value?: any): void {
            if (this.promise.$$state.status) {
                return;
            }
            this.promise.$$state.value = value;
            this.promise.$$state.status = QStatus.Rejected;
            scheduleProcessQueue(this.promise.$$state);
        }

        notify(state?: any): void {
            var pending = this.promise.$$state.pending;
            if (pending && pending.length && !this.promise.$$state.status) {
                callLater(function () {
                    _.forEach(pending, function (handlers) {
                        var deferred: IDeferred<any> = handlers[0];
                        var progressBack = handlers[QStatus.Notify];
                        try {
                            if (_.isFunction(progressBack)) {
                                state = progressBack(state);
                            }
                            deferred.notify(state);
                        } catch (e) {
                            console.log(e);
                        }
                    });
                });
            }
        }

        promise: Promise<T>;
    }

    function defer<T>(): IDeferred<T> {
        return new Deferred<T>();
    }

    function when<T>(value: T | IPromise<T>): IPromise<T>;
    function when<TResult, T>(value: T | IPromise<T>, successCallback: (promiseValue: T) => TResult | IPromise<TResult>, errorCallback?: (reason: any) => any, notifyCallback?: (state: any) => any): IPromise<TResult>;
    function when(): IPromise<void>;
    function when(this: void, value?: any, successCallback?: any, errorCallback?: any, notifyCallback?: any): IPromise<any> {
        var d = defer();
        d.resolve(value);
        return d.promise.then(successCallback, errorCallback, notifyCallback);
    }

    function all<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>, T4 | IPromise<T4>, T5 | IPromise<T5>, T6 | IPromise<T6>, T7 | IPromise<T7>, T8 | IPromise<T8>, T9 | IPromise<T9>, T10 | IPromise<T10>]): IPromise<[T1, T2, T3, T4, T5, T6, T7, T8, T9, T10]>;
    function all<T1, T2, T3, T4, T5, T6, T7, T8, T9>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>, T4 | IPromise<T4>, T5 | IPromise<T5>, T6 | IPromise<T6>, T7 | IPromise<T7>, T8 | IPromise<T8>, T9 | IPromise<T9>]): IPromise<[T1, T2, T3, T4, T5, T6, T7, T8, T9]>;
    function all<T1, T2, T3, T4, T5, T6, T7, T8>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>, T4 | IPromise<T4>, T5 | IPromise<T5>, T6 | IPromise<T6>, T7 | IPromise<T7>, T8 | IPromise<T8>]): IPromise<[T1, T2, T3, T4, T5, T6, T7, T8]>;
    function all<T1, T2, T3, T4, T5, T6, T7>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>, T4 | IPromise<T4>, T5 | IPromise<T5>, T6 | IPromise<T6>, T7 | IPromise<T7>]): IPromise<[T1, T2, T3, T4, T5, T6, T7]>;
    function all<T1, T2, T3, T4, T5, T6>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>, T4 | IPromise<T4>, T5 | IPromise<T5>, T6 | IPromise<T6>]): IPromise<[T1, T2, T3, T4, T5, T6]>;
    function all<T1, T2, T3, T4, T5>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>, T4 | IPromise<T4>, T5 | IPromise<T5>]): IPromise<[T1, T2, T3, T4, T5]>;
    function all<T1, T2, T3, T4>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>, T4 | IPromise<T4>]): IPromise<[T1, T2, T3, T4]>;
    function all<T1, T2, T3>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>, T3 | IPromise<T3>]): IPromise<[T1, T2, T3]>;
    function all<T1, T2>(values: [T1 | IPromise<T1>, T2 | IPromise<T2>]): IPromise<[T1, T2]>;
    function all<TAll>(promises: IPromise<TAll>[]): IPromise<TAll[]>;
    function all(promises: { [id: string]: IPromise<any>; }): IPromise<{ [id: string]: any; }>;
    function all<T extends {}>(promises: { [id: string]: IPromise<any>; }): IPromise<T>;
    function all(this: void, promises: any): IPromise<any> {
        var results = _.isArray(promises) ? [] : {};
        var counter = 0;
        var d = new Deferred();
        _.forEach(promises, (promise: IPromise<any>, index: number) => {
            counter++;
            when(promise).then(function (value) {
                results[index] = value;
                counter--;
                if (!counter) {
                    d.resolve(results);
                }
            }, function (rejection) {
                d.reject(rejection);
            });
        });
        if (!counter) {
            d.resolve(results);
        }
        return d.promise;
    }

    function reject(this: void, reason?: any): IPromise<any> {
        var d = defer();
        d.reject(reason);
        return d.promise;
    }

    function resolve<T>(value: T | IPromise<T>): IPromise<T>;
    function resolve(): IPromise<void>;
    function resolve(this: void, value?: any): IPromise<any> {
        return when(value);
    }

    var $Q: any = function <T>(resolver: (resolve: IQResolveReject<T>, reject: IQResolveReject<any>) => any): IPromise<T> {
        if (!_.isFunction(resolver)) {
            throw "Expect function, got" + resolver;
        }
        var d = defer();
        resolver(<IQResolveReject<any>>_.bind(d.resolve, d), <IQResolveReject<any>>_.bind(d.reject, d));
        return d.promise;
    }

    $Q.all = all;
    $Q.when = when;
    $Q.defer = defer;
    $Q.resolve = resolve;
    $Q.reject = reject;
    return $Q;
}
export class $QProvider implements IQProvider {
    $get: any;
    constructor() {
        this.$get = ["$rootScope", function ($rootScope: IRootScopeService) {
            return qFactory(function (callback: () => void) {
                $rootScope.$evalAsync(callback);
            });
        }];
    }
    errorOnUnhandledRejections(): boolean;
    errorOnUnhandledRejections(value: boolean): IQProvider;
    errorOnUnhandledRejections(value?: boolean): IQProvider | boolean {
        return <any>this;
    }
}
export class $$QProvider implements IQProvider {
    errorOnUnhandledRejections(): boolean;
    errorOnUnhandledRejections(value: boolean): IQProvider;
    errorOnUnhandledRejections(value?: any): IQProvider | boolean {
        throw new Error('Method not implemented.');
    }

    $get: any;

    constructor() {
        this.$get = function () {
            return qFactory(function (callback: () => void) {
                setTimeout(callback);
            });
        };
    }
}