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
    });
});