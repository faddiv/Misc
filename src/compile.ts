import * as _ from "lodash";
import { ICompileProvider, IDirectiveFactory, auto } from "angular";

"use strict";

export default function $CompileProvider($provide: auto.IProvideService) {

    var hasDirectives: any = {};

    this.directive = function (name: string, directiveFactory: IDirectiveFactory) {
        if (!hasDirectives.hasOwnProperty(name)) {
            hasDirectives[name] = [];
            $provide.factory(name + "Directive", ["$injector", function ($injector: auto.IInjectorService) {
                var factories = hasDirectives[name];
                return _.map(factories, $injector.invoke);
            }]);
        }
        hasDirectives[name].push(directiveFactory);
    }
    this.$get = function () {
        return {};
    };
}

$CompileProvider.$inject = ["$provide"];