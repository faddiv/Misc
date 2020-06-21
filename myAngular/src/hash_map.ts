import * as _ from 'lodash';
"use strict";

export function hashKey(value: any) {
    var type = typeof value;
    var uid;
    if (
        type === "function" ||
        (type === "object" && value !== null)) {
        uid = value.$$hashKey;
        if (typeof uid === "function") {
            uid = value.$$hashKey();
        } else if (uid === undefined) {
            uid = value.$$hashKey = _.uniqueId();
        }
    } else {
        uid = value;
    }
    return type + ":" + uid;
}

export class HashMap {
    constructor() {

    }
    put(key: any, value: any) {
        this[hashKey(key)] = value;
    }
    get(key: any) {
        return this[hashKey(key)];
    }
    remove(key: any) {
        key = hashKey(key);
        var value = this[key];
        delete this[key];
        return value;
    }
}