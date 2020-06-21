import * as sinon from 'sinon';
import * as _ from "lodash";
import { setupModuleLoader } from '../../src/loader';
import { createInjector } from '../../src/injector';
import {
    auto, IInterpolateProvider, IInterpolateService
} from "angular";
import { publishExternalAPI } from "../../src/angular_public";

describe("$interpolate", () => {
    var injector: auto.IInjectorService;
    var $interpolate: IInterpolateService;
    beforeEach(() => {
        publishExternalAPI();
        injector = createInjector(["ng"]);
        $interpolate = injector.get("$interpolate");
    });

    afterEach(() => {
        delete window.angular;
    });

    it("should exist", () => {
        var injector = createInjector(["ng"]);
        expect(injector.has("$interpolate")).toBe(true);
        expect(injector.get("$interpolate")).toBeDefined();
    });

    it("procudes an identity function for static content", () => {
        var interp = $interpolate("hello");
        expect(interp instanceof Function).toBe(true);
        expect(interp({/*context*/ })).toEqual("hello");
    });

    it("evulates a single expression", () => {
        var interp = $interpolate("{{anAttr}}");
        expect(interp({ anAttr: "42" })).toEqual("42");
    });

    it("evulates many expression (with static parts)", () => {
        var scope = { attr1: "rabbit", attr2: 42 };
        var interp = $interpolate("a {{attr1}} is a {{attr2}}.");
        expect(interp(scope)).toEqual("a rabbit is a 42.");

        var interp = $interpolate("{{attr1}}{{attr2}}");
        expect(interp(scope)).toEqual("rabbit42");
    });

    it("passes through ill-defined interpolations", () => {
        var interp = $interpolate("why u no }}work{{?");
        expect(interp({ work: "42" })).toEqual("why u no }}work{{?");
    });

    //Value Stringification

    it("turns nulls into empty strings", () => {
        var interp = $interpolate("{{nv}}");
        expect(interp({ nv: null })).toEqual("");
    });

    it("turns undefineds into empty strings", () => {
        var interp = $interpolate("{{nv}}");
        expect(interp({})).toEqual("");
    });

    it("turns numbers into string", () => {
        var interp = $interpolate("{{se}}");
        expect(interp({ se: 42 })).toEqual("42");
    });

    it("turns booleans into string", () => {
        var interp = $interpolate("{{se}}");
        expect(interp({ se: true })).toEqual("true");
    });

    it("turns arrays into JSON strings", () => {
        var interp = $interpolate("{{av}}");
        expect(interp({ av: [1, 2, [3]] })).toEqual("[1,2,[3]]");
    });

    it("turns objects into JSON strings", () => {
        var interp = $interpolate("{{av}}");
        expect(interp({ av: { a: 42, b: { c: "d" } } })).toEqual("{\"a\":42,\"b\":{\"c\":\"d\"}}");
    });
    //Supporting Escaped Interpolation Symbols
    it("unescapes escapesd sequences", () => {
        var interp = $interpolate("\\{\\{expr\\}\\} {{expr}} \\}\\}expr\\{\\{");
        expect(interp({ expr: "value" })).toEqual("{{expr}} value }}expr{{");
    });
    //Skipping Interpolation When There Are No Expressions
    it("does not return function when flagged and no expressions", () => {
        var interp = $interpolate("static", true);
        expect(interp).toBeFalsy();
    });
    
    it("returns function when flagged and has expressions", () => {
        var interp = $interpolate("sm {{expr}}", true);
        expect(interp).not.toBeFalsy();
    });
    //Text Node Interpolation
    
    it("returns function when flagged and has expressions", () => {
        var interp = $interpolate("sm {{expr}}", true);
        expect(interp).not.toBeFalsy();
    });
});