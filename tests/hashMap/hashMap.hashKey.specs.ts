import * as _ from 'lodash';
import { hashKey } from '../../src/hash_map';

describe("hash", () => {
    "use strict";

    describe("hashKey", function () {
        it("is undefined:undefined for undefined", () => {
            expect(hashKey(undefined)).toEqual("undefined:undefined");
        });

        it("is object:null for null", () => {
            expect(hashKey(null)).toEqual("object:null");
        });

        it("is boolean:true for true", () => {
            expect(hashKey(true)).toEqual("boolean:true");
        });

        it("is boolean:false for false", () => {
            expect(hashKey(false)).toEqual("boolean:false");
        });

        it("is number:42 for 42", () => {
            expect(hashKey(42)).toEqual("number:42");
        });

        it("is string:42 for '42'", () => {
            expect(hashKey("42")).toEqual("string:42");
        });

        it("is object:[unique id] for objects", () => {
            expect(hashKey({})).toMatch(/^object:\S+$/);
        });

        it("is the same key when asked for the same object many times", () => {
            var obj = {};
            expect(hashKey(obj)).toEqual(hashKey(obj));
        });

        it("does not change when object value changes", () => {
            var obj = { a: 42 };
            var hash1 = hashKey(obj);
            obj.a = 43;
            var hash2 = hashKey(obj);
            expect(hash1).toEqual(hash2);
        });

        it("is not the same for different objects even with the same value", () => {
            var obj1 = { a: 42 };
            var obj2 = { a: 42 };
            expect(hashKey(obj1)).not.toEqual(hashKey(obj2));
        });

        it("is function:[unique id] for function", () => {
            var fn = function (a) { return a; };
            expect(hashKey(fn)).toMatch(/^function:\S+$/);
        });

        it("is the same key when asked for the same function many times", () => {
            var fn = function (a) { return a; };
            expect(hashKey(fn)).toEqual(hashKey(fn));
        });

        it("is not the same for different identical functions", () => {
            var fn1 = function (a) { return a; };
            var fn2 = function (a) { return a; };
            expect(hashKey(fn1)).not.toEqual(hashKey(fn2));
        });

        it("stores the hash key in the $$hashKey attribute", () => {
            var obj: any = { a: 42 };
            var hash = hashKey(obj);
            expect(obj.$$hashKey).toEqual(hash.match(/object:(\S+)$/)[1]);
        });

        it("uses preassigned $$hashKey", () => {
            expect(hashKey({ $$hashKey: 42 })).toEqual("object:42");
        });

        it("supports a function $$hashKey", () => {
            expect(hashKey({ $$hashKey: _.constant(42) })).toEqual("object:42");
        });

        it("calls the function $$hashKey as a method with the correct this", () => {
            expect(hashKey({
                myKey: 42,
                $$hashKey() {
                    return this.myKey;
                }
            })).toEqual("object:42");
        });
    });
});