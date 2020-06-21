import { HashMap } from '../../src/hash_map';

describe("hash", () => {
    "use strict";

    describe("HashMap", function () {
        it("supports put and get of primitives", () => {
            var map = new HashMap();
            map.put(42, "fourty two");
            expect(map.get(42)).toEqual("fourty two");
        });

        it("supports put and get of objects with hashKey semantics", () => {
            var map = new HashMap();
            var obj = {};
            map.put(obj, "my value");
            expect(map.get(obj)).toEqual("my value");
            expect(map.get({})).toBeUndefined();
        });

        it("supports remove", () => {
            var map = new HashMap();
            map.put(42, "fourty two");
            map.remove(42);
            expect(map.get(42)).toBeUndefined();
        });
        
        it("returns value from remove", () => {
            var map = new HashMap();
            map.put(42, "fourty two");
            expect(map.remove(42)).toEqual("fourty two");
        });
    });
});