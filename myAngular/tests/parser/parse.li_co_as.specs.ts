import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
"use strict";

describe("parse", () => {
    var parse: IParseService;
    beforeEach(() => {
        publishExternalAPI();
        var injector = createInjector(["ng"]);
        parse = injector.get("$parse");
    });

    it("marks integer literal", () => {
        var fn = parse("42");
        expect(fn.literal).toBe(true);
    });

    it("marks string literal", () => {
        var fn = parse("'42'");
        expect(fn.literal).toBe(true);
    });

    it("marks boolean literal", () => {
        var fn = parse("false");
        expect(fn.literal).toBe(true);
    });

    it("marks arrays literal", () => {
        var fn = parse("[1,2,aValue]");
        expect(fn.literal).toBe(true);
    });

    it("marks objects literal", () => {
        var fn = parse("{a: 1, b: 2, c: aValue}");
        expect(fn.literal).toBe(true);
    });

    it("marks unary expressions non-literal", () => {
        var fn = parse("!false");
        expect(fn.literal).toBe(false);
    });

    it("marks binary expressions non-literal", () => {
        var fn = parse("2 + 2");
        expect(fn.literal).toBe(false);
    });

    it("marks integer constant", () => {
        var fn = parse("42");
        expect(fn.constant).toBe(true);
    });

    it("marks string constant", () => {
        var fn = parse("'42'");
        expect(fn.constant).toBe(true);
    });

    it("marks boolean constant", () => {
        var fn = parse("false");
        expect(fn.constant).toBe(true);
    });

    it("marks identifiers non-constant", () => {
        var fn = parse("a");
        expect(fn.constant).toBe(false);
    });

    it("marks arrays constant when elements are constant", () => {
        var fn = parse("[1,2,3]");
        expect(fn.constant).toBe(true);
        fn = parse("[1,[2,[3]]]");
        expect(fn.constant).toBe(true);
        fn = parse("[1,2,a]");
        expect(fn.constant).toBe(false);
        fn = parse("[1,[2,[a]]]");
        expect(fn.constant).toBe(false);
    });

    it("marks object constant when values are constant", () => {
        var fn = parse("{a:1,b:2,c:3}");
        expect(fn.constant).toBe(true);
        fn = parse("{a:1,bc:{b:2,cc:{c:3}}}");
        expect(fn.constant).toBe(true);
        fn = parse("{a:1,b:2,c:a}");
        expect(fn.constant).toBe(false);
        fn = parse("{a:1,bc:{b:2,cc:{c:a}}}");
        expect(fn.constant).toBe(false);
    });

    it("marks this as non-constant", () => {
        var fn = parse("this");
        expect(fn.constant).toBe(false);
        fn = parse("{a:this}");
        expect(fn.constant).toBe(false);
    });
    //Extra test for $locals
    it("marks $locals as non-constant", () => {
        var fn = parse("$locals");
        expect(fn.constant).toBe(false);
        fn = parse("{a:$locals}");
        expect(fn.constant).toBe(false);
    });

    it("marks non-computed lookup constant when object is constant", () => {
        var fn = parse("{a:1}.a");
        expect(fn.constant).toBe(true);
        fn = parse("obj.a");
        expect(fn.constant).toBe(false);
    });

    it("marks computed lookup constant when object and key are constant", () => {
        var fn = parse("{a:1}['a']");
        expect(fn.constant).toBe(true);
        fn = parse("obj['a']");
        expect(fn.constant).toBe(false);
        fn = parse("{a:1}[a]");
        expect(fn.constant).toBe(false);
        fn = parse("obj[a]");
        expect(fn.constant).toBe(false);
    });

    it("marks function calls non-constant", () => {
        var fn = parse("aFunction()");
        expect(fn.constant).toBe(false);
    });

    it("marks filters constant if arguments are", () => {
        var module = window.angular.module("my", ["ng"]);
        module.filter("aFilter", () => {
                    return a => a;
                });
        var injector = createInjector(["my"]);
        var parse = injector.get("$parse");
        var fn = parse("[1,2,3]|aFilter");
        expect(fn.constant).toBe(true);
        var fn = parse("[1,2,a]|aFilter");
        expect(fn.constant).toBe(false);
        var fn = parse("[1,2,3]|aFilter:42");
        expect(fn.constant).toBe(true);
        var fn = parse("[1,2,3]|aFilter:a");
        expect(fn.constant).toBe(false);
    });

    it("marks assigments constant when both sides are", () => {
        var fn = parse("1 = 2");
        expect(fn.constant).toBe(true);
        fn = parse("a = 2");
        expect(fn.constant).toBe(false);
        fn = parse("1 = b");
        expect(fn.constant).toBe(false);
        fn = parse("a = b");
        expect(fn.constant).toBe(false);
    });

    it("marks unaries constant when arguments are constant", () => {
        var fn = parse("+42");
        expect(fn.constant).toBe(true);
        var fn = parse("+a");
        expect(fn.constant).toBe(false);
    });

    it("marks binaries constant when both arguments are constant", () => {
        var fn = parse("1 + 42");
        expect(fn.constant).toBe(true);
        var fn = parse("1 + a");
        expect(fn.constant).toBe(false);
        var fn = parse("a + 2");
        expect(fn.constant).toBe(false);
        var fn = parse("a + b");
        expect(fn.constant).toBe(false);
    });

    it("marks logicals constant when both arguments are constant", () => {
        var fn = parse("true && false");
        expect(fn.constant).toBe(true);
        var fn = parse("true && a");
        expect(fn.constant).toBe(false);
        var fn = parse("a && false");
        expect(fn.constant).toBe(false);
        var fn = parse("a && b");
        expect(fn.constant).toBe(false);
    });

    it("marks tenaries constant when all arguments are constant", () => {
        var fn = parse("true ? 1 : 2");
        expect(fn.constant).toBe(true);
        fn = parse("a ? 1 : 2");
        expect(fn.constant).toBe(false);
        fn = parse("true ? a : 2");
        expect(fn.constant).toBe(false);
        fn = parse("true ? 1 : a");
        expect(fn.constant).toBe(false);
        fn = parse("a ? a : a");
        expect(fn.constant).toBe(false);
    });
});