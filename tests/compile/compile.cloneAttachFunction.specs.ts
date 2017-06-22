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

    describe("clone attach function", () => {
        //The Clone Attach Function
        it("can be passed to public link fn", () => {
            var injector = makeInjectorWithDirectives({});
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div></div>");
                var myScope = $rootScope.$new();
                var gotEl: JQuery;
                var gotScope: IScope;

                $compile(el)(myScope, function cloneAttachFn(element, scope) {
                    gotEl = element;
                    gotScope = scope;
                });

                expect(gotEl[0].isEqualNode(el[0])).toBe(true);
                expect(gotScope).toBe(myScope);
            });
        });

        it("causes compiled elements to be cloned", () => {
            var injector = makeInjectorWithDirectives({});
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div>Hello</div>");
                var myScope = $rootScope.$new();
                var gotClonedEl: JQuery;

                $compile(el)(myScope, function cloneAttachFn(element, scope) {
                    gotClonedEl = element;
                });

                expect(gotClonedEl[0].isEqualNode(el[0])).toBe(true);
                expect(gotClonedEl[0]).not.toBe(el[0]);
            });
        });

        it("causes cloned DOM to be linked", () => {
            var gotCompileEl: JQuery, gotLinkEl: JQuery;
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        compile(compileEl) {
                            gotCompileEl = compileEl;
                            return function link(scope, linkEl) {
                                gotLinkEl = linkEl;
                            }
                        }
                    };
                },
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");
                var myScope = $rootScope.$new();

                $compile(el)(myScope, function () { });

                expect(gotCompileEl[0].isEqualNode(gotLinkEl[0])).toBe(true);
                expect(gotCompileEl[0]).not.toBe(gotLinkEl[0]);
            });
        });

        it("allows connecting transcluded content", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        template: "<div in-template></div>",
                        link(scope: IScope, ellement: JQuery, attrs, ctrl, transcludeFn: ITranscludeFunction) {
                            var myScope = scope.$new();
                            transcludeFn(scope, function (transcludeNode: JQuery) {
                                ellement.find("[in-template]").append(transcludeNode);
                            });
                        }
                    };
                },
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div in-transclude></div></div>");

                $compile(el)($rootScope);

                expect(el.find("> [in-template] > [in-transclude]").length).toBe(1);
            });
        });

        it("can be used as the only transclusion function argument", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        template: "<div in-template></div>",
                        link(scope, element: JQuery, attrs, ctrl, transcludeFn: ITranscludeFunction) {
                            transcludeFn(function (transcludeNode: JQuery) {
                                element.find("[in-template]").append(transcludeNode);
                            })
                        }
                    };
                },
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div in-transclude></div></div>");

                $compile(el)($rootScope);

                expect(el.find("> [in-template] > [in-transclude]").length).toBe(1);
            });
        });

        it("allows passing data to transclusion", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        template: "<div in-template></div>",
                        link(scope, element: JQuery, attrs, ctrl, transcludeFn: ITranscludeFunction) {
                            transcludeFn(function (transcludeNode: JQuery, transcludeScope: IScope) {
                                transcludeScope.data = "Hy";
                                element.find("[in-template]").append(transcludeNode);
                            })
                        }
                    };
                },
                myOtherDirective() {
                    return {
                        link(scope: IScope, element: JQuery) {
                            element.html(scope.data);
                        }
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div my-other-directive></div></div>");

                $compile(el)($rootScope);

                expect(el.find("> [in-template] > [my-other-directive]").html()).toEqual("Hy");
            });
        });
    });
});