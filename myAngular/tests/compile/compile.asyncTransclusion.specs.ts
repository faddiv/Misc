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
    var xhr: Sinon.SinonFakeXMLHttpRequest;
    var requests: Sinon.SinonFakeXMLHttpRequest[] = [];

    beforeEach(function () {
        delete window.angular;
        publishExternalAPI();
        ng = window.angular;
        myModule = ng.module("myModule", []);

        xhr = sinon.useFakeXMLHttpRequest();
        xhr.onCreate = function (req) {
            requests.push(req)
        }
    });

    afterEach(() => {
        xhr.restore();
        requests.splice(0, requests.length);
        delete window.angular;
    });

    function makeInjectorWithDirectives(name: any, ...params: object[]) {
        var args = arguments;
        return createInjector(["ng", function ($compileProvider: ICompileProvider) {
            $compileProvider.directive.apply($compileProvider, args);
        }]);
    }

    describe("with transclusion", () => {

        //Transclusion with Template URLs
        it("works when template arrives first", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        templateUrl: templateUrl,
                        link(scope: IScope, element: JQuery, attrs: IAttributes, ctrl: any, transclude: ITranscludeFunction) {
                            element.find("[in-template]").append(transclude());
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div in-transclude></div></div>");

                var linkFunction = $compile(el);
                $rootScope.$apply();

                requests[0].respond(200, {}, "<div in-template></div>");
                linkFunction($rootScope);

                expect(el.find("> [in-template] > [in-transclude]").length).toBe(1);
            });
        });

        it("works when template arrives after", () => {
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        transclude: true,
                        templateUrl: templateUrl,
                        link(scope: IScope, element: JQuery, attrs: IAttributes, ctrl: any, transclude: ITranscludeFunction) {
                            element.find("[in-template]").append(transclude());
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder><div in-transclude></div></div>");

                var linkFunction = $compile(el);
                $rootScope.$apply();
                linkFunction($rootScope);
                requests[0].respond(200, {}, "<div in-template></div>");

                expect(el.find("> [in-template] > [in-transclude]").length).toBe(1);
            });
        });

        it("is only allowed once", () => {
            var otherCompileSpy = jasmine.createSpy("otherCompileSpy");
            var injector = makeInjectorWithDirectives({
                myTranscluder() {
                    return {
                        priority: 1,
                        transclude: true,
                        templateUrl: templateUrl,
                    };
                },
                mySecondTranscluder() {
                    return {
                        priority: 0,
                        transclude: true,
                        compile: otherCompileSpy
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-transcluder my-second-transcluder></div>");

                $compile(el);
                $rootScope.$apply();
                requests[0].respond(200, {}, "<div in-template></div>");

                expect(otherCompileSpy).not.toHaveBeenCalled();
            });
        });

    });
});