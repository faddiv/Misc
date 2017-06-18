"use strict";
import * as _ from "lodash";
import * as sinon from 'sinon';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes } from "angular";
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

    describe("transclude", () => {

        //Basic Transclusion
        it("removes the children of the element from the DOM", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div>Must go</div></div>");

                var linkFunction = $compile(el);
                linkFunction($rootScope);

                expect(el.is(":empty")).toBe(true);
            });
        });

        it("compiles child elements", () => {
            var insideCompileSpy = jasmine.createSpy("insideCompileSpy");
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true
                    };
                },
                insideTranscluder() {
                    return {
                        compile: insideCompileSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div inside-transcluder></div></div>");

                $compile(el);

                expect(insideCompileSpy).toHaveBeenCalled();
            });
        });

        it("makes contents available to directive link function", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        template: "<div in-template></div>",
                        link(scope, element: JQuery, attrs, ctrl, transclude: Function) {
                            element.find("[in-template]").append(transclude());
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div in-transcluder></div></div>");

                var linkFn = $compile(el);
                linkFn($rootScope);

                expect(el.find("> [in-template] > [in-transcluder]").length).toBe(1);
            });
        });

        it("is only allowed once per element", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true
                    };
                },
                mySecondTranscluder() {
                    return {
                        transclude: true
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder my-second-transcluder></div>");

                expect(function () {
                    $compile(el);
                }).toThrow();
            });
        });

        //
        it("makes scope available to link functions inside", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        link(scope, element, attrs, ctrl, transclude) {
                            element.append(transclude())
                        }
                    };
                },
                myInnerDirective() {
                    return {
                        link(scope, element) {
                            element.html(scope.anAttr);
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div my-inner-directive></div></div>");

                $rootScope.anAttr = "Hello from root";
                $compile(el)($rootScope);
                expect(el.find("> [my-inner-directive]").html()).toBe("Hello from root");
            });
        });

        it("does not use the inherited scope of the directive", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        scope: true,
                        link(scope, element, attrs, ctrl, transclude) {
                            scope.anAttr = "Shadowed attribute"
                            element.append(transclude())
                        }
                    };
                },
                myInnerDirective() {
                    return {
                        link(scope, element) {
                            element.html(scope.anAttr);
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div my-inner-directive></div></div>");

                $rootScope.anAttr = "Hello from root";
                $compile(el)($rootScope);
                expect(el.find("> [my-inner-directive]").html()).toBe("Hello from root");
            });
        });

        it("stops watching when transcluding directive is destroyed", () => {
            var watchSpy = jasmine.createSpy("watchSpy");
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        scope: true,
                        link(scope: IScope, element, attrs, ctrl, transclude) {
                            element.append(transclude())
                            scope.$on("destroyNow", function () {
                                scope.$destroy();
                            });
                        }
                    };
                },
                myInnerDirective() {
                    return {
                        link(scope: IScope, element) {
                            scope.$watch(watchSpy);
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div my-inner-directive></div></div>");
                $compile(el)($rootScope);
                $rootScope.$apply();
                expect(watchSpy.calls.count()).toBe(2);
                $rootScope.$apply();
                expect(watchSpy.calls.count()).toBe(3);
                $rootScope.$broadcast("destroyNow");
                $rootScope.$apply();
                expect(watchSpy.calls.count()).toBe(3);
            });
        });
        
        it("allows passing another scope to transclusion function", () => {
            var otherLinkSpy = jasmine.createSpy("otherLinkSpy");
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        scope: {},
                        template: "<div></div>",
                        link(scope: IScope, element, attrs, ctrl, transclude) {
                            var mySpecialScope = scope.$new(true);
                            mySpecialScope.specialAttr = 42;
                            transclude(mySpecialScope);
                        }
                    };
                },
                myOtherDirective() {
                    return {
                        link: otherLinkSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div my-other-directive></div></div>");

                $compile(el)($rootScope);

                var transcludedScope = otherLinkSpy.calls.first().args[0];
                expect(transcludedScope.specialAttr).toBe(42);
            });
        });
    });
});