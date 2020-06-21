"use strict";
import { publishExternalAPI } from "./angular_public";
import { createInjector } from "./injector";
import * as _ from "lodash";
import { default as $ } from "jquery";
import { IAngularBootstrapConfig, auto, ICompileService, IScope } from "angular";

publishExternalAPI();

window.angular.bootstrap = function (
    element: string | Element | JQuery | Document,
    modules?: Array<string | Function | (string | Function)[]>,
    config?: IAngularBootstrapConfig): auto.IInjectorService {
    modules = modules || [];
    config = config || {};
    modules.unshift(["$provide", function ($provide: auto.IProvideService) {
        $provide.value("$rootElement", $element);
    }]);
    modules.unshift("ng");
    var $element = $(element);
    var injector = createInjector(modules, config.strictDi);
    $element.data("$injector", injector);
    injector.invoke(["$compile", "$rootScope", function ($compile: ICompileService, $rootScope: IScope) {
        $rootScope.$apply(function () {
            $compile($element)($rootScope);
        });
    }]);
    return injector;
}

var ngPrefixes = ["ng-", "data-ng-", "ng:", "x-ng-"];
export default function angularReadyCallback() {
    var foundAppElement: Element;
    var foundModule: string;
    var config: IAngularBootstrapConfig = {};
    _.forEach(ngPrefixes, function (prefix) {
        var attrName = prefix + "app";
        var selector = "[" + attrName.replace(":", "\\:") + "]";
        var element: Element;
        if (!foundModule && (element = document.querySelector(selector))) {
            foundAppElement = element;
            foundModule = element.getAttribute(attrName);
        }
    });
    if (foundAppElement) {
        config.strictDi = _.some(ngPrefixes, function (prefix) {
            var attrName = prefix + "strict-di";
            return foundAppElement.hasAttribute(attrName);
        });
        window.angular.bootstrap(
            foundAppElement,
            foundModule ? [foundModule] : [],
            config
        );
    }
}
$(document).ready(angularReadyCallback);