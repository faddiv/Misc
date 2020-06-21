import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IParseService } from "angular";
"use strict";

describe("filter filter", () => {
    var parse: IParseService;

    beforeEach(() => {
        publishExternalAPI();
        var injector = createInjector(["ng"]);
        parse = injector.get("$parse");
    });
    it("is available", () => {

        var injector = createInjector(["ng"]);
        var $filter = injector.get("$filter");

        expect($filter("filter")).toBeDefined();
        expect(injector.get("filterFilter")).toBeDefined();
    });

    it("can filter an array with a predicate function", () => {
        var fn = parse("array | filter:isOdd");
        var scope = {
            array: [1, 2, 3, 4],
            isOdd: function (n: number) {
                return n % 2 !== 0;
            }
        };
        expect(fn(scope)).toEqual([1, 3]);
    });
    it("can filter an array of strings with a string", () => {
        var fn = parse("array | filter:'a'");
        var scope = {
            array: ["a", "b", "a"]
        };
        expect(fn(scope)).toEqual(["a", "a"]);
    });

    it("filters an array of strings with substring matching", () => {
        var fn = parse("array | filter:'o'");
        var scope = {
            array: ["quick", "brown", "fox"]
        };
        expect(fn(scope)).toEqual(["brown", "fox"]);
    });

    it("filters an array of strings ignoring case", () => {
        var fn = parse("array | filter:'o'");
        var scope = {
            array: ["quick", "BROWN", "fox"]
        };
        expect(fn(scope)).toEqual(["BROWN", "fox"]);
    });

    it("filters an array of objects where any value matches", () => {
        var fn = parse("array | filter:'o'");
        var scope = {
            array: [
                { firstName: "John", lastName: "Brown" },
                { firstName: "Jane", lastName: "Fox" },
                { firstName: "Mary", lastName: "Quick" },
            ]
        };
        expect(fn(scope)).toEqual([
            { firstName: "John", lastName: "Brown" },
            { firstName: "Jane", lastName: "Fox" }
        ]);
    });

    it("filters an array of objects where nested value matches", () => {
        var fn = parse("array | filter:'o'");
        var scope = {
            array: [
                { name: { first: "John", last: "Brown" } },
                { name: { first: "Jane", last: "Fox" } },
                { name: { first: "Mary", last: "Quick" } },
            ]
        };
        expect(fn(scope)).toEqual([
            { name: { first: "John", last: "Brown" } },
            { name: { first: "Jane", last: "Fox" } }
        ]);
    });

    it("filters an array of array where nested value matches", () => {
        var fn = parse("array | filter:'o'");
        var scope = {
            array: [
                [
                    { name: "John" },
                    { name: "Mary" }
                ], [
                    { name: "Jane" }
                ]
            ]
        };
        expect(fn(scope)).toEqual([
            [{ name: "John" }, { name: "Mary" }]
        ]);
    });

    it("filters with a number", () => {
        var fn = parse("array | filter: 42");
        var scope = {
            array: [
                { name: "John", age: 43 },
                { name: "Mary", age: 42 },
                { name: "Jane", age: 41 }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: "Mary", age: 42 }
        ]);
    });

    it("filters with a boolean value", () => {
        var fn = parse("array | filter: true");
        var scope = {
            array: [
                { name: "John", admin: true },
                { name: "Mary", admin: true },
                { name: "Jane", admin: false }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: "John", admin: true },
            { name: "Mary", admin: true }
        ]);
    });

    it("filters matching null", () => {
        var fn = parse("array | filter: null");
        var scope = {
            array: ["not null",
                null,
                0
            ]
        };
        expect(fn(scope)).toEqual([null]);
    });

    it("does not match null value with the string null", () => {
        var fn = parse("array | filter: 'null'");
        var scope = {
            array: ["not null",
                null,
                0
            ]
        };
        expect(fn(scope)).toEqual(["not null"]);
    });

    it("does not match undefined values", () => {
        var fn = parse("array | filter: undefined");
        var scope = {
            array: ["undefined",
                undefined,
                0
            ]
        };
        expect(fn(scope)).toEqual(["undefined"]);
    });

    it("allows negating string filter", () => {
        var fn = parse("array | filter: '!o'");
        var scope = {
            array: ["quick",
                "brown",
                "fox"
            ]
        };
        expect(fn(scope)).toEqual(["quick"]);
    });

    it("filters with an object", () => {
        var fn = parse("array | filter: {name: 'o', role: 'm'}");
        var scope = {
            array: [
                { name: "John", role: "admin" },
                { name: "Mary", role: "moderator" },
                { name: "Jane", role: "admin" }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: "John", role: "admin" }
        ]);
    });

    it("matches everything when filtered with an empty object", () => {
        var fn = parse("array | filter: {}");
        var scope = {
            array: [
                { name: "John", admin: "admin" },
                { name: "Mary", admin: "moderator" }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: "John", admin: "admin" },
            { name: "Mary", admin: "moderator" }
        ]);
    });

    it("filters with a nested object", () => {
        var fn = parse("array | filter: {name: { first: 'o'}}");
        var scope = {
            array: [
                { name: { first: "Joe" }, role: "admin" },
                { name: { first: "Jane" }, role: "moderator" }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: { first: "Joe" }, role: "admin" }
        ]);
    });

    it("allows negation when filtering with an object", () => {
        var fn = parse("array | filter: {name: {first:'!o'}}");
        var scope = {
            array: [
                { name: { first: "Joe" }, role: "admin" },
                { name: { first: "Jane" }, role: "moderator" }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: { first: "Jane" }, role: "moderator" }
        ]);
    });

    it("filters with a nested object in array", () => {
        var fn = parse("array | filter: { users: {name: {first: 'o'}} }");
        var scope = {
            array: [
                {
                    users: [
                        { name: { first: "Joe" }, role: "admin" },
                        { name: { first: "Jane" }, role: "moderator" }
                    ]
                },
                {
                    users: [
                        { name: { first: "Mary" }, role: "admin" }
                    ]
                }
            ]
        };
        expect(fn(scope)).toEqual([
            {
                users: [
                    { name: { first: "Joe" }, role: "admin" },
                    { name: { first: "Jane" }, role: "moderator" }
                ]
            }
        ]);
    });
    it("filters with nested objects on the same level only", () => {
        var fn = parse("array | filter: { user: {name: 'Bob'} }");
        var scope = {
            array: [
                { user: "Bob" },
                { user: { name: "Bob" } },
                { user: { name: { first: "Bob", last: "Fox" } } }
            ]
        };
        expect(fn(scope)).toEqual([
            { user: { name: "Bob" } }
        ]);
    });
    it("filters with a wildcard property", function () {
        var fn = parse("array | filter:{$: 'o'}");
        var scope = {
            array: [
                { name: "Joe", role: "admin" },
                { name: "Jane", role: "moderator" },
                { name: "Mary", role: "admin" }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: "Joe", role: "admin" },
            { name: "Jane", role: "moderator" }
        ]);
    });
    it("filters nested objects with a wildcard property", function () {
        var fn = parse("array | filter:{$: 'o'}");
        var scope = {
            array: [
                { name: { first: "Joe" }, role: "admin" },
                { name: { first: "Jane" }, role: "moderator" },
                { name: { first: "Mary" }, role: "admin" }
            ]
        }
        expect(fn(scope)).toEqual([
            { name: { first: "Joe" }, role: "admin" },
            { name: { first: "Jane" }, role: "moderator" }
        ]);
    });
    it("filters wildcard properties scoped to parent", function () {
        var fn = parse("array | filter:{name: {$: 'o'}}");
        var scope = {
            array: [
                { name: { first: "Joe", last: "Fox" }, role: "admin" },
                { name: { first: "Jane", last: "Quick" }, role: "moderator" },
                { name: { first: "Mary", last: "Brown" }, role: "admin" }
            ]
        }
        expect(fn(scope)).toEqual([
            { name: { first: "Joe", last: "Fox" }, role: "admin" },
            { name: { first: "Mary", last: "Brown" }, role: "admin" }
        ]);
    });
    it("filters primitives with a wildcard property", function () {
        var fn = parse("array | filter:{$: 'o'}");
        var scope = { array: ["Joe", "Jane", "Mary"] };
        expect(fn(scope)).toEqual(["Joe"]);
    });

    it("filters with a nested wildcard property", function () {
        var fn = parse("array | filter:{$: {$: 'o'}}");
        var scope = {
            array: [
                { name: { first: "Joe" }, role: "admin" },
                { name: { first: "Jane" }, role: "moderator" },
                { name: { first: "Mary" }, role: "admin" }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: { first: "Joe" }, role: "admin" }
        ]);
    });
    it("allows using a custom comparator", function () {
        var fn = parse("array | filter:{$: 'o'}:myComparator");
        var scope = {
            array: ["o", "oo", "ao", "aa"],
            myComparator: function (left, right) {
                return left === right;
            }
        };
        expect(fn(scope)).toEqual(["o"]);
    });
    it("allows using an equality comparator", function () {
        var fn = parse("array | filter:{name: 'Jo'}:true");
        var scope = {
            array: [
                { name: "Jo" },
                { name: "Joe" }
            ]
        };
        expect(fn(scope)).toEqual([
            { name: "Jo" }
        ]);
    });
});