"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, IScope, ICompileService } from "angular";

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

    //Looking Up A Controller Constructor from The Scope
    it("allows looking up controller from surrounding scope", () => {
        var gotScope: IScope;
            function MyController($scope: IScope) {
                gotScope = $scope;
            }
            var injector = createInjector(["ng"]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div ng-controller='MyCtrlOnScope as ctrl'>");
                $rootScope.MyCtrlOnScope = MyController;
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotScope.ctrl).toBeDefined();
                expect(gotScope.ctrl instanceof MyController).toBe(true);
            });
    });
});
