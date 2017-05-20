"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IAttributes } from "angular";
import $ from "jquery";

describe("$compile", () => {
    var ng: IAngularStatic;
    var myModule: IModule;
    beforeEach(function () {
        delete window.angular;
        publishExternalAPI();
        ng = window.angular;
        myModule = ng.module("myModule", []);
    });
    afterEach(() => {
        delete window.angular;
    });

    function makeInjectorWithDirectives(name: any, ...params: object[]) {
        var args = arguments;
        return createInjector(["ng", function ($compileProvider: ICompileProvider) {
            $compileProvider.directive.apply($compileProvider, args);
        }]);
    }

    describe("attributes", () => {

        function registerAndCompile(domString: string, callback: (el: JQuery, givenAttrs: IAttributes) => void, dirName = "myDirective") {
            var givenAttrs: IAttributes;
            var injector = makeInjectorWithDirectives(dirName, function () {
                return {
                    restrict: "EACM",
                    compile(element: JQuery, attrs: IAttributes) {
                        givenAttrs = attrs;
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService) {
                var el = $(domString);
                $compile(el);

                callback(el, givenAttrs)
            });
        }

        it("passes the element attributes to the compile function", () => {
            registerAndCompile(
                '<my-directive my-attr="1" my-other-attr="two"></my-directive>',
                (el, givenAttrs) => {
                    expect(givenAttrs.myAttr).toEqual("1");
                    expect(givenAttrs.myOtherAttr).toEqual("two");
                }
            );
        });

        it("trims attribute values", () => {
            registerAndCompile(
                '<my-directive my-attr=" 1 "></my-directive>',
                (el, givenAttrs) => {
                    expect(givenAttrs.myAttr).toEqual("1");
                    expect(givenAttrs.myOtherAttr).toEqual("two");
                }
            );
        });
        //Handling Boolean Attributes
    });
});