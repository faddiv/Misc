import * as _ from 'lodash';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';

"use strict";

describe("parse", () => {
    var parse: IParseService;
    beforeEach(() => {
        publishExternalAPI();
        var myModule = window.angular.module("myModule", ["ng"]);
        myModule.filter("uppercase", () => {
            return function (str: string) {
                return str.toUpperCase();
            }
        });

        myModule.filter("exclamate", () => {
            return function (str: string) {
                return str + "!";
            }
        });

        myModule.filter("repeat", () => {
            return function (str: string, times: number) {
                return _.repeat(str, times);
            }
        });

        myModule.filter("surround", () => {
            return function (str: string, left: string, right: string) {
                return left + str + right;
            }
        });

        var injector = createInjector(["myModule"]);
        parse = injector.get("$parse");
    });

    it("can parse filter expressions", () => {

        var fn = parse("aString | uppercase");
        expect(fn({ aString: "Hello!" })).toEqual("HELLO!");
    });

    it("can parse filter chain expressions", () => {

        var fn = parse("aString | uppercase | exclamate");
        expect(fn({ aString: "Hello" })).toEqual("HELLO!");
    });

    it("can pass an additional argument to filters", () => {
        var fn = parse("aString | repeat:3");
        expect(fn({ aString: "hello " })).toEqual("hello hello hello ");
    });

    it("can pass several additional argument to filters", () => {
        var fn = parse("aString | surround:'<':'>'");
        expect(fn({ aString: "div" })).toEqual("<div>");
    });
});