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
    });
});