"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IAttributes, IScope } from "angular";
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

        function registerAndCompile(domString: string, callback: (el: JQuery, givenAttrs: IAttributes, $rootScope: IScope) => void, dirName = "myDirective") {
            var givenAttrs: IAttributes;
            var injector = makeInjectorWithDirectives(dirName, function () {
                return {
                    restrict: "EACM",
                    compile(element: JQuery, attrs: IAttributes) {
                        givenAttrs = attrs;
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $(domString);
                $compile(el);

                callback(el, givenAttrs, $rootScope);
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
                }
            );
        });

        //Handling Boolean Attributes
        it("sets the value of boolean attributes to true", () => {
            registerAndCompile(
                '<input my-directive disabled>',
                (el, attrs) => {
                    expect(attrs.disabled).toEqual(true);
                }
            );
        });

        it("does not set the value of custom boolean attributes to true", () => {
            registerAndCompile(
                '<input my-directive disabled>',
                (el, attrs) => {
                    expect(attrs.disabled).toEqual(true);
                }
            );
        });

        //Overriding attributes with ng-attr
        it("overrides attributes with n-attr- versions", () => {
            registerAndCompile(
                '<my-directive ng-attr-whatever="42" whatever="41"></my-directive>',
                (el, attrs) => {
                    expect(attrs.whatever).toEqual("42");
                }
            )
        });

        //Setting Attributes
        it("allows setting attributes", () => {
            registerAndCompile(
                '<my-directive attr="true"></my-directive>',
                (el, attrs: IAttributes) => {
                    attrs.$set("attr", "false")
                    expect(attrs.attr).toEqual("false");
                }
            );
        });

        it("sets attributes to DOM", () => {
            registerAndCompile(
                '<my-directive attr="true"></my-directive>',
                (el, attrs: IAttributes) => {
                    attrs.$set("attr", "false")
                    expect(el.attr("attr")).toEqual("false");
                }
            );
        });

        it("does not set attributes to DOM when flag is false", () => {
            registerAndCompile(
                '<my-directive attr="true"></my-directive>',
                (el, attrs: IAttributes) => {
                    attrs.$set("attr", "false", false);
                    expect(el.attr("attr")).toEqual("true");
                }
            );
        });

        it("shares attributes between directives", () => {
            var attrs1, attrs2;
            var injector = makeInjectorWithDirectives({
                myDir() {
                    return {
                        compile(element, attrs) {
                            attrs1 = attrs;
                        }
                    }
                },
                myOtherDir() {
                    return {
                        compile(element, attrs) {
                            attrs2 = attrs;
                        }
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService) {
                var el = $('<div my-dir my-other-dir></div>');
                $compile(el);
                expect(attrs1).toBe(attrs2);
            });
        });
        //Setting Boolean Properties
        it("sets prop for boolean attributes", () => {
            registerAndCompile(
                '<input my-directive>',
                (el, attrs: IAttributes) => {
                    attrs.$set("disabled", true);
                    expect(el.prop("disabled")).toBe(true);
                }
            );
        });
        it("sets prop for boolean attributes even when not flushing", () => {
            registerAndCompile(
                '<input my-directive>',
                (el, attrs: IAttributes) => {
                    attrs.$set("disabled", true, false);
                    expect(el.prop("disabled")).toBe(true);
                }
            );
        });
        //Denormalizing Attribute Names for The DOM

        it("denormailzes attribute name when explicitly given", () => {
            registerAndCompile(
                '<my-directive some-attribute="42">',
                (el, attrs: IAttributes) => {
                    attrs.$set("someAttribute", 43, true, "some-attribute");
                    expect(el.attr("some-attribute")).toEqual("43");
                }
            );
        });

        it("denormailzes attribute by snake-casing", () => {
            registerAndCompile(
                '<my-directive some-attribute="42">',
                (el, attrs: IAttributes) => {
                    attrs.$set("someAttribute", 43);
                    expect(el.attr("some-attribute")).toEqual("43");
                }
            );
        });

        it("denormalizes attribute by using original attribute name", () => {
            registerAndCompile(
                '<my-directive x-some-attribute="42">',
                (el, attrs: IAttributes) => {
                    attrs.$set("someAttribute", 43);
                    expect(el.attr("x-some-attribute")).toEqual("43");
                }
            );
        });

        it("does not use ng-attr- perfix in denormalized names", () => {
            registerAndCompile(
                '<my-directive ng-attr-some-attribute="42">',
                (el, attrs: IAttributes) => {
                    attrs.$set("someAttribute", 43);
                    expect(el.attr("some-attribute")).toEqual("43");
                }
            );
        });

        it("uses new attribute name after once given", () => {
            registerAndCompile(
                '<my-directive x-some-attribute="42">',
                (el, attrs: IAttributes) => {
                    attrs.$set("someAttribute", 43, true, "some-attribute");
                    attrs.$set("someAttribute", 44);

                    expect(el.attr("some-attribute")).toEqual("44");
                    expect(el.attr("x-some-attribute")).toEqual("42");
                }
            );
        });
        //Observing Attributes
        it("calls observer immediately when attribute is $set", () => {
            registerAndCompile(
                '<my-directive some-attribute="42">',
                (el, attrs: IAttributes) => {
                    var gotValue;
                    attrs.$observe("someAttribute", value => {
                        gotValue = value;
                    });

                    attrs.$set("someAttribute", "43");

                    expect(gotValue).toEqual("43");
                }
            );
        });

        it("calls observer on next $digest after registration", () => {
            registerAndCompile(
                '<my-directive some-attribute="42">',
                (el, attrs: IAttributes, $rootScope: IScope) => {
                    var gotValue;
                    attrs.$observe("someAttribute", value => {
                        gotValue = value;
                    });

                    $rootScope.$digest();

                    expect(gotValue).toEqual("42");
                }
            );
        });

        it("lets observers be deregistered", () => {
            registerAndCompile(
                '<my-directive some-attribute="42">',
                (el, attrs: IAttributes) => {
                    var gotValue;
                    var remove = attrs.$observe("someAttribute", value => {
                        gotValue = value;
                    });

                    attrs.$set("someAttribute", "43");
                    expect(gotValue).toEqual("43");

                    remove();
                    attrs.$set("someAttribute", "44");
                    expect(gotValue).toEqual("43");
                }
            );
        });

        //Providing Class Directives As Attributes
        it("adds an attribute from a class directive", () => {
            registerAndCompile(
                '<div class="my-directive">',
                (el, attrs: IAttributes) => {
                    expect(attrs.hasOwnProperty("myDirective")).toBe(true);
                }
            );
        });
        
        it("does not add attribute from class without a directive", () => {
            registerAndCompile(
                '<my-directive class="some-class">',
                (el, attrs: IAttributes) => {
                    expect(attrs.hasOwnProperty("myDirective")).toBe(false);
                    expect(attrs.hasOwnProperty("someClass")).toBe(false);
                }
            );
        });

        it("supports values for class directive attributes", () => {
            registerAndCompile(
                '<div class="my-directive: my attribute value">',
                (el, attrs: IAttributes) => {
                    expect(attrs.myDirective).toEqual("my attribute value");
                }
            );
        });
        
        it("terminates class directive attribute value at semicolon", () => {
            registerAndCompile(
                '<div class="my-directive: my attribute value; some-other-class">',
                (el, attrs: IAttributes) => {
                    expect(attrs.myDirective).toEqual("my attribute value");
                }
            );
        });
    });
});