import * as _ from 'lodash';
import { IScope, IRootScopeService, IAngularEvent } from "angular";
import { IWatcher, IListenerContainer, IAsyncQueueItem, IParseService, IScopeInternal, IRootScopeProvider } from "./angularInterfaces";
"use strict";

function initWatchVal() { }

export default <IRootScopeProvider><any>function $RootScopeProvider() {
    var TTL = 10;
    this.digestTtl = function (value: number) {
        if (_.isNumber(value)) {
            TTL = value;
        }
        return TTL;
    }
    this.$get = ["$parse", function ($parse: IParseService) {
        class Scope implements IScopeInternal {
            public $id: number;
            public $$watchers: IWatcher[] = [];
            private $$lastDirtyWatch: IWatcher = null;
            private $$asyncQueue: IAsyncQueueItem[] = [];
            private $$applyAsyncQueue: Function[] = [];
            private $$applyAsynId: number = null;
            private $$postDigestQueue: Function[] = [];
            public $$phase: string = null;
            private $$children: Scope[] = [];
            private logger = console;
            public $root: IRootScopeService = this;
            public $parent: IScope;
            public $$listeners: IListenerContainer = {};
            public $$isolateBindings: any;

            constructor() {
            }

            public $digest() {
                var ttl = TTL;
                var dirty;
                this.$root.$$lastDirtyWatch = null;
                this.$$beginPhase("$digest");

                if (this.$root.$$applyAsynId) {
                    clearTimeout(this.$root.$$applyAsynId);
                    this.$$flushApplyAsync();
                }
                do {
                    while (this.$$asyncQueue.length) {
                        try {
                            var asyncTask = this.$$asyncQueue.shift();
                            asyncTask.scope.$eval(asyncTask.expression);
                        } catch (e) {
                            this.logger.error(e);
                        }
                    }
                    dirty = this.$$digestOnce();
                    if ((dirty || this.$$asyncQueue.length) && !(ttl--)) {
                        throw TTL + ' digest iterations reached';
                    }
                } while (dirty || this.$$asyncQueue.length);
                this.$$clearPhase();

                while (this.$$postDigestQueue.length) {
                    try {
                        this.$$postDigestQueue.shift()();
                    } catch (e) {
                        this.logger.error(e);
                    }
                }
            }

            public $watch(watchExpression: string, listener?: string, objectEquality?: boolean): () => void;
            public $watch<T>(watchExpression: string, listener?: (newValue: T, oldValue: T, scope: IScope) => any, objectEquality?: boolean): () => void;
            public $watch(watchExpression: (scope: IScope) => any, listener?: string, objectEquality?: boolean): () => void;
            $watch<T>(watchExpression: (scope: IScope) => T, listener?: (newValue: T, oldValue: T, scope: IScope) => any, objectEquality?: boolean): () => void;
            public $watch(watchFn: string | ((scope: IScope) => void),
                listenerFn?: string | ((oldValue: any, newValue: any, scope: IScope) => void),
                valueEq?: boolean): () => void {
                var realListenerFn = $parse(listenerFn);

                var realWatchFn = $parse(watchFn);
                if (realWatchFn.$$watchDelegate) {
                    return realWatchFn.$$watchDelegate(this, realListenerFn, valueEq, realWatchFn);
                }
                var watcher: IWatcher = {
                    watchFn: realWatchFn,
                    listenerFn: realListenerFn || initWatchVal,// || function(){}
                    valueEq: !!valueEq,
                    last: initWatchVal
                };
                this.$$watchers.unshift(watcher);
                this.$root.$$lastDirtyWatch = null;
                return () => {
                    var index = this.$$watchers.indexOf(watcher);
                    if (index >= 0) {
                        this.$$watchers.splice(index, 1);
                        this.$root.$$lastDirtyWatch = null;
                    }
                };
            }

            public $watchCollection(watchFn: string | ((scope: IScope) => void), listenerFn?: (oldValue: any, newValue: any, scope: IScope) => void, valueEq?: boolean) {
                var newValue,
                    oldValue,
                    oldLength,
                    veryOldValue,
                    trackVeryOldValue = (listenerFn.length > 1),
                    changeCount = 0,
                    firstRun = true,
                    realWatchFn = $parse(watchFn);

                var internalWatchFn = (scope) => {
                    var newLength;
                    newValue = realWatchFn(scope);

                    if (_.isObject(newValue)) {
                        if (this.isArrayLike(newValue)) {
                            if (!_.isArray(oldValue)) {
                                changeCount++;
                                oldValue = [];
                            }
                            if (newValue.length !== oldValue.length) {
                                changeCount++;
                                oldValue.length = newValue.length;
                            }
                            _.forEach(newValue, (newItem, i) => {
                                if (!this.$$areEqual(newItem, oldValue[i], false)) {
                                    changeCount++;
                                    oldValue[i] = newItem;
                                }
                            });
                        } else {
                            if (!_.isObject(oldValue) || this.isArrayLike(oldValue)) {
                                changeCount++;
                                oldValue = {};
                                oldLength = 0;
                            }
                            newLength = 0;
                            _.forOwn(newValue, (newVal, key) => {
                                newLength++;
                                if (oldValue.hasOwnProperty(key)) {
                                    if (!this.$$areEqual(oldValue[key], newVal, false)) {
                                        changeCount++;
                                        oldValue[key] = newVal;
                                    }
                                } else {
                                    changeCount++;
                                    oldLength++;
                                    oldValue[key] = newVal;
                                }
                            });
                            if (oldLength > newLength) {
                                changeCount++;
                                _.forOwn(oldValue, (oldVal, key) => {
                                    if (!newValue.hasOwnProperty(key)) {
                                        oldLength--;
                                        changeCount++;
                                        delete oldValue[key];
                                    }
                                });
                            }
                        }
                    } else {
                        if (!this.$$areEqual(newValue, oldValue, false)) {
                            changeCount++;
                        }
                        oldValue = newValue;
                    }

                    return changeCount;
                };

                var internalListenerFn = () => {
                    if (firstRun) {
                        listenerFn(newValue, newValue, this);
                        firstRun = false;
                    } else {
                        listenerFn(newValue, veryOldValue, this);
                    }

                    if (trackVeryOldValue) {
                        veryOldValue = _.clone(newValue);
                    }
                };

                return this.$watch(internalWatchFn, internalListenerFn);
            }

            public $watchGroup(watchFns: (string | ((scope: IScope) => void))[], listenerFn?: (oldValue: any[], newValue: any[], scope: IScope) => void, valueEq?: boolean) {
                var self = this;
                var newValues = new Array(watchFns.length);
                var oldValues = new Array(watchFns.length);
                var changeReactionScheduled = false;
                var firstRun = true;

                if (watchFns.length === 0) {
                    var shouldCall = true;
                    self.$evalAsync(() => {
                        if (shouldCall) {
                            listenerFn(newValues, newValues, self);
                        }
                    });
                    return () => {
                        shouldCall = false;
                    };
                }
                function watchGroupListener() {
                    if (firstRun) {
                        firstRun = false;
                        listenerFn(newValues, newValues, self);
                    } else {
                        listenerFn(newValues, oldValues, self);
                    }
                    changeReactionScheduled = false;
                }

                var destroyFunctions = _.map(watchFns, (watchFn, i) => {
                    return self.$watch(<any>watchFn, (newValue, oldValue) => {//anyCast
                        newValues[i] = newValue;
                        oldValues[i] = oldValue;
                        if (!changeReactionScheduled) {
                            changeReactionScheduled = true;
                            self.$evalAsync(watchGroupListener);
                        }
                    });
                });
                return () => {
                    _.forEach(destroyFunctions, destroyFunction => {
                        destroyFunction();
                    })
                };
            }

            public $eval(): any;
            public $eval(expression: string, locals?: Object): any;
            public $eval(expression: (scope: IScope) => any, locals?: Object): any;
            public $eval<C,R>(expression?: string | ((scope: IScope, locals?: C) => R), locals?: C): R {
                return $parse(expression)(this, locals);
            }

            public $evalAsync(): void;
            public $evalAsync(expression: string): void;
            public $evalAsync(expression: (scope: IScope, locals?: any) => any): void;
            public $evalAsync(expression?: string | ((scope: IScope, locals?: any) => any)): void {
                if (!this.$$phase && !this.$$asyncQueue.length) {
                    setTimeout(() => {
                        if (this.$$asyncQueue.length) {
                            this.$root.$digest();
                        }
                    }, 0);
                }
                this.$$asyncQueue.push({
                    scope: this,
                    expression: $parse(expression)
                });
            }

            public $applyAsync(): any;
            public $applyAsync(expression: string): any;
            public $applyAsync(expression: (scope: IScope) => any): any;
            public $applyAsync(expression?: string | ((scope: IScope, locals?: any) => any)): void {
                this.$$applyAsyncQueue.push(() => {
                    this.$eval(<any>expression);//anyCast
                });
                if (this.$root.$$applyAsynId === null) {
                    this.$root.$$applyAsynId = setTimeout(() => {
                        this.$apply(<any>_.bind(this.$$flushApplyAsync, this));
                    }, 0);
                }
            }

            public $apply(): any;
            public $apply(expression: string): any;
            public $apply(expression: (scope: IScope, locals?: any) => any): any;
            public $apply<C>(expression?: string | ((scope: IScope, locals?: any) => C)): C {
                try {
                    this.$$beginPhase("$apply");
                    return this.$eval(<any>expression);//anyCast
                } finally {
                    this.$$clearPhase();
                    this.$root.$digest();
                }
            }

            public $new(isolated?: boolean, parent?: IScope): IScope {
                var child: Scope;
                parent = parent || this;
                if (isolated) {
                    child = new Scope();
                    child.$root = parent.$root;
                    child.$$asyncQueue = parent.$$asyncQueue;
                    child.$$postDigestQueue = parent.$$postDigestQueue;
                    child.$$applyAsyncQueue = parent.$$applyAsyncQueue;
                } else {
                    child = Object.create(this);
                }
                parent.$$children.push(child);
                child.$$watchers = [];
                child.$$listeners = {};
                child.$$children = [];
                child.$parent = parent;
                return child;
            }

            public $destroy() {
                this.$broadcast("$destroy");
                if (this.$parent) {
                    var siblings = this.$parent.$$children;
                    var index = siblings.indexOf(this);
                    if (index >= 0) {
                        siblings.splice(index, 1);
                    }
                }
                this.$$watchers = null;
                this.$$listeners = {};
            }

            public $on(eventName: string, listener: (event: IAngularEvent, ...args: any[]) => any): () => void {
                var listeners: ((event: IAngularEvent, ...args: any[]) => any)[] = this.$$listeners[eventName];
                if (!listeners) {
                    this.$$listeners[eventName] = listeners = [];
                }
                listeners.push(listener);
                return () => {
                    var index = listeners.indexOf(listener);
                    if (index >= 0) {
                        listeners[index] = null;
                    }
                };
            }

            public $emit(eventName: string, ...args: any[]): IAngularEvent {
                var scope: IScope = this;
                var propagationStopped = false;
                var event: IAngularEvent = {
                    name: eventName,
                    targetScope: this,
                    currentScope: this,
                    stopPropagation: () => {
                        propagationStopped = true;
                    },
                    defaultPrevented: false,
                    preventDefault: function () {
                        this.defaultPrevented = true;
                    }
                };
                var listenerArgs = [event].concat(args);
                do {
                    event.currentScope = scope;
                    scope.$$fireEventOnScope(eventName, listenerArgs);
                    scope = scope.$parent;
                } while (scope && !propagationStopped);
                event.currentScope = null;
                return event;
            }

            public $broadcast(eventName: string, ...args: any[]): IAngularEvent {
                var event: IAngularEvent = {
                    name: eventName,
                    targetScope: this,
                    currentScope: this,
                    defaultPrevented: false,
                    preventDefault: function () {
                        this.defaultPrevented = true;
                    }
                };
                var listenerArgs = [event].concat(args);
                this.$$everyScope(scope => {
                    event.currentScope = scope;
                    scope.$$fireEventOnScope(eventName, listenerArgs);
                    return true;
                });
                event.currentScope = null;
                return event;
            }

            private $$fireEventOnScope(eventName: string, listenerArgs: any[]) {

                var listeners = this.$$listeners[eventName] || [];
                var i = 0;
                while (i < listeners.length) {
                    if (listeners[i] === null) {
                        listeners.splice(i, 1);
                    } else {
                        try {
                            listeners[i].apply(null, listenerArgs);
                        } catch (e) {
                            this.logger.error(e);
                        }
                        i++;
                    }
                }
            }

            public $$postDigest(fn: () => void) {
                this.$$postDigestQueue.push(fn);
            }

            private $$beginPhase(phase: string) {
                if (this.$$phase) {
                    throw this.$$phase + " already in progress.";
                }
                this.$$phase = phase;
            }

            private $$clearPhase() {
                this.$$phase = null;
            }

            private $$areEqual(newValue, oldValue, valueEq): boolean {
                if (valueEq) {
                    return _.isEqual(newValue, oldValue);
                } else {
                    return newValue === oldValue ||
                        (typeof (newValue) === "number" && typeof (oldValue) === "number" &&
                            isNaN(newValue) && isNaN(oldValue));
                }
            }

            private isArrayLike(obj: any): boolean {
                if (_.isNull(obj) || _.isUndefined(obj)) {
                    return false;
                }
                var length = obj.length;
                return length === 0 ||
                    (_.isNumber(length) && length > 0 && (length - 1) in obj);
            }

            private $$digestOnce(): boolean {
                var dirty: boolean;
                var continueLoop = true;
                this.$$everyScope(scope => {
                    _.forEachRight(scope.$$watchers, watcher => {
                        try {
                            if (watcher) {
                                var newValue = watcher.watchFn(scope),
                                    oldValue = watcher.last,
                                    valueEq = watcher.valueEq;
                                if (!scope.$$areEqual(newValue, oldValue, valueEq)) {
                                    this.$root.$$lastDirtyWatch = watcher;
                                    watcher.last = valueEq ? _.cloneDeep(newValue) : newValue;
                                    watcher.listenerFn(newValue, (oldValue === initWatchVal ? newValue : oldValue), scope);
                                    dirty = true;
                                } else if (this.$root.$$lastDirtyWatch === watcher) {
                                    continueLoop = false;
                                    return false;
                                }
                            }
                        } catch (e) {
                            this.logger.error(e);
                        }
                    });
                    return continueLoop;
                });
                return dirty;
            }

            private $$everyScope(fn: (scope: Scope) => boolean) {
                if (fn(this)) {
                    return this.$$children.every(child => {
                        return child.$$everyScope(fn);
                    })
                } else {
                    return false;
                }
            }

            private $$flushApplyAsync() {
                while (this.$$applyAsyncQueue.length) {
                    try {
                        this.$$applyAsyncQueue.shift()();
                    } catch (e) {
                        this.logger.log(e);
                    }
                }
                this.$root.$$applyAsynId = null;
            }
        }
        return new Scope();
    }];
}

