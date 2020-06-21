"use strict";
import * as _ from "lodash";
import * as sinon from 'sinon';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes, ITranscludeFunction } from "angular";
import { IScopeEx } from "../../typings/testInterfaces";

describe("$compile", () => {
    var ng: IAngularStatic;
    var myModule: IModule;
    var templateUrl = "/my_directive.html";
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

    describe("element transclusion", () => {

        //Full Element Transclusion
        it("removes the element from the DOM", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element'
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder></div></div>");

                $compile(el);

                expect(el.is(":empty")).toBe(true);
            });
        });

        it("replaces the element with a comment", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element'
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder></div></div>");

                $compile(el);

                expect(el.html()).toEqual("<!-- myTranscluder:  -->");
            });
        });

        it("inlcudes directive attribute value in comment", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element'
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder='42'></div></div>");

                $compile(el);

                expect(el.html()).toEqual("<!-- myTranscluder: 42 -->");
            });
        });

        it("calls directive compile and link with comment", () => {
            var gotCompiledEl: JQuery, gotLinkedEl: JQuery;
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element',
                        compile(compiledEl) {
                            gotCompiledEl = compiledEl;
                            return function (scope, linkedEl) {
                                gotLinkedEl = linkedEl;
                            }
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder></div></div>");

                $compile(el)($rootScope);

                expect(gotCompiledEl[0].nodeType).toBe(Node.COMMENT_NODE);
                expect(gotLinkedEl[0].nodeType).toBe(Node.COMMENT_NODE);
            });
        });

        it("calls lower priority compile with original", () => {
            var gotCompiledEl: JQuery;
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element',
                        priority: 2
                    };
                },
                myOtherDirective() {
                    return {
                        priority: 1,
                        compile(element) {
                            gotCompiledEl = element;
                        }
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder my-other-directive></div></div>");

                $compile(el);

                expect(gotCompiledEl[0].nodeType).toBe(Node.ELEMENT_NODE);
            });
        });

        it("calls compile on child element directive", () => {
            var compileSpy = jasmine.createSpy("compileSpy");
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element'
                    };
                },
                myOtherDirective() {
                    return {
                        compile: compileSpy
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder my-other-directive></div></div>");

                $compile(el);

                expect(compileSpy).toHaveBeenCalled();
            });
        });

        it("compiles original element contents once", () => {
            var compileSpy = jasmine.createSpy("compileSpy");
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element'
                    };
                },
                myOtherDirective() {
                    return {
                        compile: compileSpy
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder><div my-other-directive></div></div></div>");

                $compile(el);

                expect(compileSpy.calls.count()).toBe(1);
            });
        });

        it("makes original element available for transclusion", () => {
            var injector = makeInjectorWithDirectives({
                myDouble() {
                    return {
                        transclude: 'element',
                        link(scope: IScope, el: JQuery, attrs: IAttributes, ctrl, transclude: ITranscludeFunction) {
                            transclude(function (clone) {
                                el.after(clone);
                            });
                            transclude(function (clone) {
                                el.after(clone);
                            });
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-double>Hello</div>");

                $compile(el)($rootScope);

                expect(el.find("[my-double]").length).toBe(2);
            });
        });

        it("sets directive attributes element to comment", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: 'element',
                        link(scope: IScope, el: JQuery, attrs: IAttributes, ctrl, transclude: ITranscludeFunction) {
                            attrs.$set("testing", "42");
                            el.after(transclude());
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div><div my-transcluder></div></div>");

                $compile(el)($rootScope);

                expect(el.find("[my-transcluder]").attr("testing")).toBeUndefined();
            });
        });
        //Requiring Controllers from Transcluded Directives
        it("supports requiring controllers", () => {
            var MyController = function () { };
            var gotCtrl;
            var injector = makeInjectorWithDirectives({
                myCtrlDirective() {
                    return {
                        controller: MyController
                    };
                },
                myTranscluder() {
                    return {
                        transclude: 'element',
                        link(scope: IScope, el: JQuery, attrs: IAttributes, ctrl, transclude: ITranscludeFunction) {
                            el.after(transclude());
                        }
                    };
                },
                myOtherDirective() {
                    return {
                        require: "^myCtrlDirective",
                        link(scope: IScope, el: JQuery, attrs: IAttributes, ctrl, transclude: ITranscludeFunction) {
                            gotCtrl = ctrl;
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $(
                    "<div><div my-ctrl-directive my-transcluder><div my-other-directive></div></div></div>");

                $compile(el)($rootScope);

                expect(gotCtrl).toBeDefined();
                expect(gotCtrl instanceof MyController).toBe(true);
            });
        });
    });
});