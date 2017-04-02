import * as _ from 'lodash';
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

    it("can parse an integer", () => {
        var fn = parse("42");
        expect(fn).toBeDefined();
        expect(fn()).toBe(42);
    });

    it("can parse a floating point number", () => {
        var fn = parse("4.2");
        expect(fn()).toBe(4.2);
    });

    it("can parse a floating point number without an integer part", () => {
        var fn = parse(".42");
        expect(fn()).toBe(0.42);
    });

    it("can parse scientific notation with a float coefficient", () => {
        var fn = parse("42e3");
        expect(fn()).toBe(42000);
    });

    it("can parse scientific notation with negative exponents", () => {
        var fn = parse("4200e-2");
        expect(fn()).toBe(42);
    });

    it("can parse scientific notation with the + sign", () => {
        var fn = parse(".42e+2");
        expect(fn()).toBe(42);
    });

    it("can parse upper case scientific notation", () => {
        var fn = parse(".42E2");
        expect(fn()).toBe(42);
    });

    it("will not parse invalid scientific notation", () => {
        expect(() => { parse("42e-"); }).toThrow();
        expect(() => { parse("42e-a"); }).toThrow();
    });

    it("can parse a string in single quotes", () => {
        var fn = parse("'abc'");
        expect(fn()).toBe("abc");
    });

    it("can parse a string in double quotes", () => {
        var fn = parse("\"abc\"");
        expect(fn()).toBe("abc");
    });

    it("will not parse if string is not closed", () => {
        expect(() => { parse("\"abc"); }).toThrow();
        expect(() => { parse("\'abc"); }).toThrow();
    });

    it("will not parse string with mismatching quotes", () => {
        expect(() => { parse("\"abc\'"); }).toThrow();
        expect(() => { parse("\'abc\""); }).toThrow();
    });

    it("can parse a string with single quotes inside", () => {
        var fn = parse("\'a\\\'bc\'");
        expect(fn()).toBe("a\'bc");
    });

    it("can parse a string with double quotes inside", () => {
        var fn = parse("\"a\\\"bc\"");
        expect(fn()).toBe("a\"bc");
    });

    it("will parse a string with unicode escapes", () => {
        var fn = parse("\"\\u00A0\"");
        expect(fn()).toBe("\u00A0");
    });

    it("will not parse a string with invalid unicode escapes", () => {
        expect(() => { parse("\"\\u00G0\""); }).toThrow();
    });

    //TODO: En tennek meg ide string teszteket.

    it("will parse null", () => {
        var fn = parse("null");
        expect(fn()).toBe(null);
    });

    it("will parse true", () => {
        var fn = parse("true");
        expect(fn()).toBe(true);
    });

    it("will parse false", () => {
        var fn = parse("false");
        expect(fn()).toBe(false);
    });

    it("ignores whitespace", () => {
        var fn = parse(" \r\n\v\t\u00A0 42 ");
        expect(fn()).toBe(42);
    });

    it("will parse an empty array", () => {
        var fn = parse("[]");
        expect(fn()).toEqual([]);
    });

    it("will parse a non-empty array", () => {
        var fn = parse("[1, 'two', [3], true, 4.2]");
        expect(fn()).toEqual([1, "two", [3], true, 4.2]);
    });

    it("will parse an array with trailing commas", () => {
        var fn = parse("[1, 2, 3, ]");
        expect(fn()).toEqual([1, 2, 3]);
    });

    it("will parse an empty object", () => {
        var fn = parse("{}");
        expect(fn()).toEqual({});
    });
    it("will parse a non-empty object", () => {
        var fn = parse("{'a key':1,\"another-key\": 2}");
        expect(fn()).toEqual({ "a key": 1, "another-key": 2 });
    });
    it("will parse a non-empty object", () => {
        var fn = parse("{ a : 1 ,b:[2,3], c: {d: 4} }");
        expect(fn()).toEqual({ a: 1, b: [2, 3], c: { d: 4 } });
    });
    it("looks up an attribute from the scope", () => {
        var fn = parse("aKey");
        expect(fn({ aKey: 42 })).toBe(42);
    });
    it("returns undefined when looking up attribute from undefined", () => {
        var fn = parse("aKey");
        expect(fn()).toBeUndefined();
    });
    //Not in the book. I think should be there.
    it("can handle more key", () => {
        var fn = parse("{b:a, c:d}");
        expect(fn({ a: 42, d: "42" })).toEqual({ b: 42, c: "42" });
    });
    //I broke up one test into two
    it("will parse this when it is defined", () => {
        var fn = parse("this");
        var scope = {};
        expect(fn(scope)).toBe(scope);
    });
    it("will parse this when it is undefined", () => {
        var fn = parse("this");
        expect(fn()).toBeUndefined();
    });
    //I broke up one test into three
    it("looks up a 2-part identifier path from scope when fully defined", () => {
        var fn = parse("aKey.anotherKey");
        expect(fn({ aKey: { anotherKey: 42 } })).toBe(42);
    });
    it("looks up a 2-part identifier path from scope when inner key undefined", () => {
        var fn = parse("aKey.anotherKey");
        expect(fn({ aKey: {} })).toBeUndefined();
    });
    it("looks up a 2-part identifier path from scope when outer key undefined", () => {
        var fn = parse("aKey.anotherKey");
        expect(fn({})).toBeUndefined();
    });
    //I think the compiled function can be optimized from this:
    //var v0;if({aKey:42}){v0=({aKey:42}).aKey;}return v0;
    //To this:
    //var v0,v1;v1 = {aKey:42};if(v1){v0=(v1).aKey;}return v0;
    it("looks up a member from an object", () => {
        var fn = parse("{aKey: 42}.aKey");
        expect(fn()).toBe(42);
    });
    it("looks up a 4-part identifier path from scope when 4 parts are defined", () => {
        var fn = parse("one.two.three.four");
        expect(fn({ one: { two: { three: { four: 42 } } } })).toBe(42);
    });
    it("looks up a 4-part identifier path from scope when 3 parts are defined", () => {
        var fn = parse("one.two.three.four");
        expect(fn({ one: { two: { three: {} } } })).toBeUndefined();
    });
    it("looks up a 4-part identifier path from scope when 2 parts are defined", () => {
        var fn = parse("one.two.three.four");
        expect(fn({ one: { two: {} } })).toBeUndefined();
    });
    it("looks up a 4-part identifier path from scope when 1 parts are defined", () => {
        var fn = parse("one.two.three.four");
        expect(fn({ one: {} })).toBeUndefined();
    });
    it("looks up a 4-part identifier path from scope when 0 parts are defined", () => {
        var fn = parse("one.two.three.four");
        expect(fn()).toBeUndefined();
    });
    it("uses locals instead of scope when there is a matching key", () => {
        var fn = parse("aKey");
        var scope = { aKey: 42 };
        var locals = { aKey: 43 };
        expect(fn(scope, locals)).toBe(43);
    });
    it("does not use locals instead of scope when no matching key", () => {
        var fn = parse("aKey");
        var scope = { aKey: 42 };
        var locals = { otherKey: 43 };
        expect(fn(scope, locals)).toBe(42);
    });
    it("uses locals instead of scope when the first part matches", () => {
        var fn = parse("aKey.anotherKey");
        var scope = { aKey: { anotherKey: 42 } };
        var locals = { aKey: {} };
        expect(fn(scope, locals)).toBeUndefined();
    });
    //I broke up one test into two
    it("will parse $locals", () => {
        var fn = parse("$locals");
        var scope = {};
        var locals = {};
        expect(fn(scope, locals)).toBe(locals);
        expect(fn(scope)).toBeUndefined();
    });
    it("will parse $locals with a property", () => {
        var fn = parse("$locals.aKey");
        var scope = { aKey: 42 };
        var locals = { aKey: 43 };
        expect(fn(scope, locals)).toBe(43);
    });
    it("parses a simple computed property access", () => {
        var fn = parse("aKey['anotherKey']");
        expect(fn({ aKey: { anotherKey: 42 } })).toBe(42);
    });
    it("parses a computed numeric array access", () => {
        var fn = parse("anArray[1]");
        expect(fn({ anArray: [1, 2, 3] })).toBe(2);
    });
    it("parses a computed access with another key as property", () => {
        var fn = parse("lock[key]");
        expect(fn({ key: "theKey", lock: { theKey: 42 } })).toBe(42);
    });
    it("parses computed access with another access as property", () => {
        var fn = parse("lock[keys['aKey']]");
        expect(fn({ keys: { aKey: "theKey" }, lock: { theKey: 42 } })).toBe(42);
    });
    it("parses a function call", () => {
        var fn = parse("aFunction()");
        expect(fn({ aFunction: function () { return 42; } })).toBe(42);
    });
    //extra test for composite function
    it("calls function if its an inner function", () => {
        var fn = parse("aKey['anotherKey'].aFunction()");
        expect(fn({ aKey: { anotherKey: { aFunction: function () { return 42; } } } })).toBe(42);
    });
    //extra test for undefined function.
    it("returns undefined if function call is undefined", () => {
        var fn = parse("aFunction()");
        expect(fn({})).toBeUndefined();
    });
    //number -> constant name change since it's more general.
    it("parses a function call with a single constant argument", () => {
        var fn = parse("aFunction(42)");
        expect(fn({ aFunction: function (n) { return n; } })).toBe(42);
    });
    it("parses a function call with a single identifier argument", () => {
        var fn = parse("aFunction(n)");
        expect(fn({ n: 42, aFunction: function (n) { return n; } })).toBe(42);
    });
    it("parses a function call with a single function call argument", () => {
        var fn = parse("aFunction(argFn())");
        expect(fn({
            aFunction: function (n) { return n; },
            argFn: _.constant(42)
        })).toBe(42);
    });
    it("parses a function call with multiple arguments", () => {
        var fn = parse("aFunction(37, n, argFn())");
        expect(fn({
            n: 3,
            aFunction: function (a1, a2, a3) { return a1 + a2 + a3; },
            argFn: _.constant(2)
        })).toBe(42);
    });
    it("calls methods accessed as computed properties", () => {
        var scope = {
            anObject: {
                aMember: 42,
                aFunction: function () {
                    return this.aMember;
                }
            }
        };
        var fn = parse("anObject['aFunction']()");
        expect(fn(scope)).toBe(42);
    });
    it("calls methods accessed as non-computed properties", () => {
        var scope = {
            anObject: {
                aMember: 42,
                aFunction: function () {
                    return this.aMember;
                }
            }
        };
        var fn = parse("anObject.aFunction()");
        expect(fn(scope)).toBe(42);
    });
    it("binds bare functions to the scope", () => {
        var scope = {
            aFunction: function () {
                return this;
            }
        };
        var fn = parse("aFunction()");
        expect(fn(scope)).toBe(scope);
    });
    it("binds bare functions on locals to the locals", () => {
        var scope = {};
        var locals = {
            aFunction: function () {
                return this;
            }
        };
        var fn = parse("aFunction()");
        expect(fn(scope, locals)).toBe(locals);
    });
    it("parses a simple attribute assignment", () => {
        var fn = parse("anAttribute = 42");
        var scope: any = {};
        expect(fn(scope)).toBe(42);
        expect(scope.anAttribute).toBe(42);
    });
    it("can assign any primary expression", () => {
        var fn = parse("anAttribute = aFunction()");
        var scope: any = {
            aFunction: _.constant(42)
        };
        expect(fn(scope)).toBe(42);
        expect(scope.anAttribute).toBe(42);
    });
    it("can assign a computed object property", () => {
        var fn = parse("anObject['anAttribute'] = 42");
        var scope: any = {
            anObject: {}
        };
        expect(fn(scope)).toBe(42);
        expect(scope.anObject.anAttribute).toBe(42);
    });
    it("can assign a non-computed object property", () => {
        var fn = parse("anObject.anAttribute = 42");
        var scope: any = {
            anObject: {}
        };
        expect(fn(scope)).toBe(42);
        expect(scope.anObject.anAttribute).toBe(42);
    });
    it("can assign a nested object property", () => {
        var fn = parse("anArray[0].anAttribute = 42");
        var scope: any = {
            anArray: [{}]
        };
        expect(fn(scope)).toBe(42);
        expect(scope.anArray[0].anAttribute).toBe(42);
    });
    it("creates the objects in the assignment path that do not exist", () => {
        var fn = parse("some['nested'].property.path = 42");
        var scope: any = {};
        expect(fn(scope)).toBe(42);
        expect(scope.some.nested.property.path).toBe(42);
    });
    //Security part
    it("does not allow calling the function constructor", () => {
        expect(() => {
            var fn = parse("aFunction.constructor('return window;')()");
            fn({ aFunction: function () { } });
        }).toThrow();
    });
    it("does not allow accessing __proto__", () => {
        expect(() => {
            var fn = parse("obj.__proto__");
            fn({ obj: {} });
        }).toThrow();
    });
    it("does not allow calling __defineGetter__", () => {
        expect(() => {
            var fn = parse("obj.__defineGetter__('evil', fn)");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __defineSetter__", () => {
        expect(() => {
            var fn = parse("obj.__defineSetter__('evil', fn)");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __lookupGetter__", () => {
        expect(() => {
            var fn = parse("obj.__lookupGetter__('evil')");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __lookupSetter__", () => {
        expect(() => {
            var fn = parse("obj.__lookupSetter__('evil')");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    //Missing tests for Identifier
    it("does not allow calling the function constructor on bare scope", () => {
        expect(() => {
            var fn = parse("constructor('return window;')()");
            fn(function () { });
        }).toThrow();
    });
    it("does not allow accessing __proto__ on bare scope", () => {
        expect(() => {
            var fn = parse("__proto__");
            fn({});
        }).toThrow();
    });
    it("does not allow calling __defineGetter__ on bare scope", () => {
        expect(() => {
            var fn = parse("__defineGetter__('evil', fn)");
            fn({ fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __defineSetter__ on bare scope", () => {
        expect(() => {
            var fn = parse("__defineSetter__('evil', fn)");
            fn({ fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __lookupGetter__ on bare scope", () => {
        expect(() => {
            var fn = parse("__lookupGetter__('evil')");
            fn({ fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __lookupSetter__ on bare scope", () => {
        expect(() => {
            var fn = parse("__lookupSetter__('evil')");
            fn({ fn: function () { } });
        }).toThrow();
    });
    //Missing tests for computed properties
    it("does not allow calling the function constructor when computed", () => {
        expect(() => {
            var fn = parse("aFunction['constructor']('return window;')()");
            fn({ aFunction: function () { } });
        }).toThrow();
    });
    it("does not allow accessing __proto__ when computed", () => {
        expect(() => {
            var fn = parse("obj['__proto__']");
            fn({ obj: {} });
        }).toThrow();
    });
    it("does not allow calling __defineGetter__ when computed", () => {
        expect(() => {
            var fn = parse("obj['__defineGetter__']('evil', fn)");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __defineSetter__ when computed", () => {
        expect(() => {
            var fn = parse("obj['__defineSetter__']('evil', fn)");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __lookupGetter__ when computed", () => {
        expect(() => {
            var fn = parse("obj['__lookupGetter__']('evil')");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    it("does not allow calling __lookupSetter__ when computed", () => {
        expect(() => {
            var fn = parse("obj['__lookupSetter__']('evil')");
            fn({ obj: {}, fn: function () { } });
        }).toThrow();
    });
    //End Missing tests
    it("does not allow accessing window as computed property", () => {
        var fn = parse("anObject['wnd']");
        expect(function () {
            fn({
                anObject: { wnd: window }
            });
        }).toThrow();
    });
    it("does not allow accessing window as non-computed property", () => {
        var fn = parse("anObject.wnd");
        expect(function () {
            fn({
                anObject: { wnd: window }
            });
        }).toThrow();
    });
    it("does not allow passing window as function argument", () => {
        var fn = parse("aFunction(wnd)");
        expect(function () {
            fn({
                aFunction: function () { },
                wnd: window
            });
        }).toThrow();
    });
    //scrollTo not callable for some reason. replaced with toString
    it("does not allow calling methods on window", () => {
        var fn = parse("wnd.toString()");
        expect(function () {
            fn({
                wnd: window
            });
        }).toThrow();
    });
    it("does not allow functions to return window", () => {
        var fn = parse("getWnd()");
        expect(function () {
            fn({
                getWnd: _.constant(window)
            });
        }).toThrow();
    });
    it("does not allow assigning window", () => {
        var fn = parse("anObject = wnd");
        expect(function () {
            fn({
                wnd: window
            });
        }).toThrow();
    });
    it("does not allow referencing window", () => {
        var fn = parse("wnd");
        expect(function () {
            fn({
                wnd: window
            });
        }).toThrow();
    });
    it("does not allow calling functions on DOM elements", () => {
        var fn = parse("el.setAttribute('evil','true')");
        expect(function () {
            fn({
                el: document.documentElement
            });
        }).toThrow();
    });
    it("does not allow calling the aliased function constructor", () => {
        var fn = parse("fnConstructor('return window;')");
        expect(function () {
            fn({
                fnConstructor: (function () { }).constructor
            });
        }).toThrow();
    });
    it("does not allow calling functions on Object", () => {
        var fn = parse("obj.create({})");
        expect(function () {
            fn({
                obj: Object
            });
        }).toThrow();
    });
    it("does not allow calling call", () => {
        var fn = parse("fun.call(obj)");
        expect(function () {
            fn({
                fun: function () { },
                obj: {}
            });
        }).toThrow();
    });
    it("does not allow calling apply", () => {
        var fn = parse("fun.apply(obj)");
        expect(function () {
            fn({
                fun: function () { },
                obj: {}
            });
        }).toThrow();
    });
    //Missing third test
    it("does not allow calling bind", () => {
        var fn = parse("fun.bind(obj)");
        expect(function () {
            fn({
                fun: function () { },
                obj: {}
            });
        }).toThrow();
    });

    it("parses a unary + on constant", () => {
        expect(parse("+42")()).toBe(42);
    });

    it("parses a unary + on property", () => {
        expect(parse("+a")({ a: 42 })).toBe(42);
    });

    it("replaces undefined with zero for unary +", () => {
        expect(parse("+a")({})).toBe(0);
    });

    it("parses a unary ! on boolean", () => {
        expect(parse("!true")({})).toBe(false);
    });

    it("parses a unary ! on number", () => {
        expect(parse("!42")({})).toBe(false);
    });

    it("parses a unary ! on property", () => {
        expect(parse("!a")({ a: false })).toBe(true);
    });

    it("parses a unary ! on another unary", () => {
        expect(parse("!!0")({})).toBe(false);
    });

    it("parses a unary - on constant", () => {
        expect(parse("-42")()).toBe(-42);
    });

    it("parses a unary - on property", () => {
        expect(parse("-a")({ a: -42 })).toBe(42);
    });

    it("parses a unary - multiple times", () => {
        expect(parse("--a")({ a: -42 })).toBe(-42);
    });

    it("parses a unary - on undefined property", () => {
        expect(parse("-a")({})).toBe(0);
    });

    it("parses a ! in a string", () => {
        expect(parse("'!'")({})).toBe("!");
    });

    it("parses a multiplication", () => {
        expect(parse("21 * 2")({})).toBe(42);
    });

    it("parses a division", () => {
        expect(parse("84 / 2")({})).toBe(42);
    });

    it("parses a remainder", () => {
        expect(parse("85 % 43")({})).toBe(42);
    });

    it("parses several multiplicatives", () => {
        expect(parse("17 * 5 % 43")({})).toBe(42);
    });

    it("parses an addition", () => {
        expect(parse("20 + 22")({})).toBe(42);
    });

    it("parses a subtraction", () => {
        expect(parse("62 - 20")({})).toBe(42);
    });

    it("parses multiplicatives on a higher precedence than additives", () => {
        expect(parse("22 + 4 * 5")({})).toBe(42);
        expect(parse("22 + 3 * 10 - 10")({})).toBe(42);
    });

    it("substitues undefined with zero in addition", () => {
        expect(parse("a + 42")({})).toBe(42);
        expect(parse("42 + a")({})).toBe(42);
    });

    it("substitues undefined with zero in substraction", () => {
        expect(parse("a - 42")({})).toBe(-42);
        expect(parse("42 - a")({})).toBe(42);
    });

    it("substitues undefined with zero in multiplication", () => {
        expect(parse("a * 42")({})).toBe(0);
        expect(parse("42 * a")({})).toBe(0);
    });

    it("parses relational operators", () => {
        expect(parse("1 < 2")({})).toBe(true);
        expect(parse("1 > 2")({})).toBe(false);
        expect(parse("1 <= 2")({})).toBe(true);
        expect(parse("2 <= 2")({})).toBe(true);
        expect(parse("1 >= 2")({})).toBe(false);
        expect(parse("2 >= 2")({})).toBe(true);
    });

    it("parses equality operators", () => {
        expect(parse("42 == 42")({})).toBe(true);
        expect(parse("42 == '42'")({})).toBe(true);
        expect(parse("42 != 42")({})).toBe(false);
        expect(parse("42 === 42")({})).toBe(true);
        expect(parse("42 === '42'")({})).toBe(false);
        expect(parse("42 !== 42")({})).toBe(false);
    });

    it("parses relations on a higher precedence than equality", () => {
        expect(parse("42 == '42' > 42 === '42'")({})).toBe(false);
    });

    it("parses additives on a higher precedence than relationals", () => {
        expect(parse("2 + 3 < 6 - 2")({})).toBe(false);
    });

    it("parses logical AND", () => {
        expect(parse("true && true")({})).toBe(true);
        expect(parse("true && false")({})).toBe(false);
        expect(parse("false && false")({})).toBe(false);
    });

    it("parses logical OR", () => {
        expect(parse("true || true")({})).toBe(true);
        expect(parse("true || false")({})).toBe(true);
        expect(parse("false || false")({})).toBe(false);
    });

    it("parses multiple ANDs", () => {
        expect(parse("true && true && true")({})).toBe(true);
        expect(parse("true && true && false")({})).toBe(false);
    });

    it("parses multiple ORs", () => {
        expect(parse("true || true || true")({})).toBe(true);
        expect(parse("true || true || false")({})).toBe(true);
        expect(parse("false || false || true")({})).toBe(true);
        expect(parse("false || false || false")({})).toBe(false);
    });

    it("short-circuits AND", () => {
        var invoked = false;
        var scope = { fn: () => invoked = true };
        expect(parse("false && fn()")(scope)).toBe(false);
        expect(invoked).toBe(false);
    });

    it("short-circuits OR", () => {
        var invoked = false;
        var scope = { fn: () => invoked = true };
        expect(parse("true || fn()")(scope)).toBe(true);
        expect(invoked).toBe(false);
    });

    it("parses AND a higher precedence than OR", () => {
        expect(parse("false && true || true")()).toBe(true);
    });

    it("parses equality a higher precedence than OR", () => {
        expect(parse("1 === 2 || 2 === 2")()).toBeTruthy();
    });

    it("parses equality a higher precedence than AND", () => {
        expect(parse("1 !== 2 && 2 === 2")()).toBeTruthy();
    });

    it("parses the ternary expression", () => {
        expect(parse("a === 42 ? 2 : 3")({ a: 42 })).toBe(2);
        expect(parse("a === 42 ? 2 : 3")({ a: 43 })).toBe(3);
    });

    it("parses OR with a higher precedence than ternary", () => {
        expect(parse("0 || 1 ? 0 || 2 : 0 || 3")({ a: 42 })).toBe(2);
        expect(parse("0 || 0 ? 0 || 2 : 0 || 3")({ a: 42 })).toBe(3);
    });

    it("parses nested ternaries", () => {
        expect(parse("a === 42 ? b === 42 ? 'a and b' : 'a' : c === 42 ? 'c' : 'none'")({
            a: 44,
            b: 43,
            c: 42
        })).toBe("c");
    });

    it("parses parentheses altering precedence order", () => {
        expect(parse("21 * (3 - 1)")()).toBe(42);
        expect(parse("false && (true || true)")()).toBe(false);
        expect(parse("-((a % 2) === 0 ? 1 : 2)")({ a: 42 })).toBe(-1);
    });

    it("parses several statements", () => {
        var fn = parse("a = 1; b = 2; c = 3");
        var scope = {};
        fn(scope);
        expect(scope).toEqual({ a: 1, b: 2, c: 3 });
    });

    it("returns the value of the last statements", () => {
        expect(parse("a = 1; b = 2; a+b")({})).toBe(3);
    });

    it("returns the function itself when given one", () => {
        var fn = function () { };
        expect(parse(fn)).toBe(fn);
    });

});