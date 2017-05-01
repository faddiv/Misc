"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule } from "angular";

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

    it("allows creating directives", () => {
        myModule.directive("testing", <IDirectiveFactory>function () { });
        var injector = createInjector(['ng', 'myModule']);
        expect(injector.has('testingDirective')).toBe(true);
    });

    it("allows creating many directives with the same name", () => {
        myModule.directive("testing", _.constant({ d: 'one' }));
        myModule.directive("testing", _.constant({ d: 'two' }));

        var injector = createInjector(["ng", "myModule"]);

        var result = injector.get<any[]>("testingDirective");
        expect(result.length).toBe(2);
        expect(result[0].d).toEqual("one");
        expect(result[2].d).toEqual("two");
    });
});