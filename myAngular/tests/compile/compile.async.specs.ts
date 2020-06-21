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

    //Basic Templating
    it("defers remaining directive compilation", () => {
        var otherCompileSpy = jasmine.createSpy("otherCompileSpy");
        var injector = makeInjectorWithDirectives({
            myDirective() {
                return {
                    templateUrl: templateUrl
                };
            },
            myOtherDirective() {
                return {
                    compile: otherCompileSpy
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive my-other-directive></div>");
            $compile(el);

            expect(otherCompileSpy).not.toHaveBeenCalled();
        });
    });

    it("defers curent directive compilation", () => {
        var compileSpy = jasmine.createSpy("compileSpy");
        var injector = makeInjectorWithDirectives({
            myDirective() {
                return {
                    templateUrl: templateUrl,
                    compile: compileSpy
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive></div>");
            $compile(el);

            expect(compileSpy).not.toHaveBeenCalled();
        });
    });

    it("immediatelly empties out the element", () => {
        var injector = makeInjectorWithDirectives({
            myDirective() {
                return {
                    templateUrl: templateUrl
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive>Aloha</div>");
            $compile(el);

            expect(el.is(":empty")).toBe(true);
        });
    });

    describe("templateUrl", () => {
        var xhr: Sinon.SinonFakeXMLHttpRequest;
        var requests: Sinon.SinonFakeXMLHttpRequest[] = [];

        beforeEach(() => {
            xhr = sinon.useFakeXMLHttpRequest();
            xhr.onCreate = function (req) {
                requests.push(req)
            }
        });

        afterEach(() => {
            xhr.restore();
            requests.splice(0, requests.length);
        });

        it("fetches the template", () => {
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");
                $compile(el);
                $rootScope.$apply();

                expect(requests.length).toBe(1);
                expect(requests[0].method).toBe("GET");
                expect(requests[0].url).toBe(templateUrl);
            });
        });

        it("populates element with template", () => {
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");
                $compile(el);
                $rootScope.$apply();

                requests[0].respond(200, {}, "<div class='from-template'></div>")
                expect(el.find("> .from-template").length).toBe(1);
            });
        });

        it("compiles current directive when template received", () => {
            var compileSpy = jasmine.createSpy("compileSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl,
                        compile: compileSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");
                $compile(el);
                $rootScope.$apply();

                requests[0].respond(200, {}, "<div class='from-template'></div>")

                expect(compileSpy).toHaveBeenCalled();
            });
        });

        it("resumes compilation when template received", () => {
            var otherCompileSpy = jasmine.createSpy("otherCompileSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl
                    };
                },
                myOtherDirective() {
                    return {
                        compile: otherCompileSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive></div>");

                $compile(el);
                $rootScope.$apply();

                requests[0].respond(200, {}, "<div class='from-template'></div>")

                expect(otherCompileSpy).toHaveBeenCalled();
            });
        });

        it("resumes child compilation after template received", () => {
            var otherCompileSpy = jasmine.createSpy("otherCompileSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl
                    };
                },
                myOtherDirective() {
                    return {
                        compile: otherCompileSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");

                $compile(el);
                $rootScope.$apply();

                requests[0].respond(200, {}, "<div my-other-directive></div>")

                expect(otherCompileSpy).toHaveBeenCalled();
            });
        });

        //Template URL Functions
        it("supports functions as values", () => {
            var templateUrlSpy = jasmine.createSpy("templateUrlSpy").and.returnValue(templateUrl);
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrlSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");

                $compile(el);
                $rootScope.$apply();

                expect(requests[0].url).toBe(templateUrl);
                var call = templateUrlSpy.calls.first();
                expect(call.args[0][0]).toBe(el[0]);
                expect(call.args[1].myDirective).toBeDefined();
            });
        });

        //Disallowing More Than One Template URL Directive Per Element
        it("does not allow templateUrl directive after template directive", () => {
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        template: "<div></div>"
                    };
                },
                myOtherDirective() {
                    return {
                        templateUrl: templateUrl
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive></div>");

                expect(function () {
                    $compile(el);
                }).toThrow();
            });
        });

        //Linking Asynchronous Directives
        it("links the directive when public link function is invoked", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl,
                        link: linkSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");

                var linkFunction = $compile(el);
                $rootScope.$apply();
                requests[0].respond(200, {}, "<div></div>")
                linkFunction($rootScope);
                expect(linkSpy).toHaveBeenCalled();
                var call = linkSpy.calls.first();
                expect(call.args[0]).toBe($rootScope);
                expect(call.args[1][0]).toBe(el[0]);
                expect(call.args[2].myDirective).toBeDefined();
            });
        });


        it("links child elements when public link function is invoked", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl
                    };
                },
                myOtherDirective() {
                    return {
                        link: linkSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");

                var linkFunction = $compile(el);
                $rootScope.$apply();
                requests[0].respond(200, {}, "<div my-other-directive></div>")

                linkFunction($rootScope);
                expect(linkSpy).toHaveBeenCalled();
                var call = linkSpy.calls.first();
                expect(call.args[0]).toBe($rootScope);
                expect(call.args[1][0]).toBe(el[0].firstChild);
                expect(call.args[2].myOtherDirective).toBeDefined();
            });
        });

        it("links when template arrives if node link fn was called", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        link: linkSpy
                    };
                },
                myOtherDirective() {
                    return {
                        templateUrl: templateUrl,
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive></div>");

                var linkFunction = $compile(el);
                $rootScope.$apply();
                linkFunction($rootScope);
                requests[0].respond(200, {}, "<div></div>");

                expect(linkSpy).toHaveBeenCalled();
                var call = linkSpy.calls.first();
                expect(call.args[0]).toBe($rootScope);
                expect(call.args[1][0]).toBe(el[0]);
                expect(call.args[2].myDirective).toBeDefined();
            });
        });
        //Linking Directives that Were Compiled Earlier
        it("link directives that were compiled earlier", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        templateUrl: templateUrl,
                        link: linkSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive></div>");

                var linkFunction = $compile(el);
                linkFunction($rootScope);
                $rootScope.$apply();
                requests[0].respond(200, {}, "<div></div>");

                expect(linkSpy).toHaveBeenCalled();
                var call = linkSpy.calls.first();
                expect(call.args[0]).toBe($rootScope);
                expect(call.args[1][0]).toBe(el[0]);
                expect(call.args[2].myDirective).toBeDefined();
            });
        });
        //Preserving The Isolate Scope Directive
        it("retains isolate scope directives from earlier", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        scope: {
                            val: "=myDirective"
                        },
                        link: linkSpy
                    };
                },
                myOtherDirective() {
                    return {
                        templateUrl: templateUrl,
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='42' my-other-directive></div>");

                var linkFunction = $compile(el);
                $rootScope.$apply();
                linkFunction($rootScope);
                requests[0].respond(200, {}, "<div></div>");

                expect(linkSpy).toHaveBeenCalled();
                var call = linkSpy.calls.first();
                expect(call.args[0]).toBeDefined();
                expect(call.args[0]).not.toBe($rootScope);
                expect(call.args[0].val).toBe(42);
            });
        });

        it("supports isolate scope directives with templateUrls", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        scope: {
                            val: "=myDirective"
                        },
                        link: linkSpy,
                        templateUrl: templateUrl
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='42' my-other-directive></div>");

                var linkFunction = $compile(el);
                $rootScope.$apply();
                linkFunction($rootScope);
                requests[0].respond(200, {}, "<div></div>");

                expect(linkSpy).toHaveBeenCalled();
                var call = linkSpy.calls.first();
                expect(call.args[0]).not.toBe($rootScope);
                expect(call.args[0].val).toBe(42);
            });
        });

        it("links children of isolate scope directives with templateUrls", () => {
            var linkSpy = jasmine.createSpy("linkSpy");
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        scope: {
                            val: "=myDirective"
                        },
                        templateUrl: templateUrl
                    };
                },
                myChildDirective() {
                    return {
                        link: linkSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='42'></div>");

                var linkFunction = $compile(el);
                linkFunction($rootScope);
                $rootScope.$apply();
                requests[0].respond(200, {}, "<div my-child-directive></div>");

                expect(linkSpy).toHaveBeenCalled();
                var call = linkSpy.calls.first();
                expect(call.args[0]).not.toBe($rootScope);
                expect(call.args[0].val).toBe(42);
            });
        });
        //Preserving Controller Directives
        it("sets up controllers for all controller directives", () => {
            var myDirectiveControllerInstantiated: boolean;
            var myOtherDirectiveControllerInstantiated: boolean;
            var injector = makeInjectorWithDirectives({
                myDirective() {
                    return {
                        controller: function MyDirectiveController() {
                            myDirectiveControllerInstantiated = true;
                        }
                    };
                },
                myOtherDirective() {
                    return {
                        templateUrl: templateUrl,
                        controller: function myOtherDirectiveController() {
                            myOtherDirectiveControllerInstantiated = true;
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive></div>");

                var linkFunction = $compile(el);
                linkFunction($rootScope);
                $rootScope.$apply();
                requests[0].respond(200, {}, "<div></div>");

                expect(myDirectiveControllerInstantiated).toBe(true);
                expect(myOtherDirectiveControllerInstantiated).toBe(true);
            });
        });
    });
});