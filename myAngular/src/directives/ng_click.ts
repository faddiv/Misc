import { IScope, IAttributes } from "angular";
import { IParseService } from "../angularInterfaces";

"use strict";

export default function ngClickDirective($parse: IParseService) {
    return {
        restrict: "A",
        compile(el: JQuery, attrs: IAttributes) {
            var clickFn = $parse(attrs.ngClick);
            return function link(scope: IScope, element: JQuery, attrs: IAttributes) {
                element.on("click", function (evt: Event) {
                    scope.$eval(clickFn, { $event: evt });
                    scope.$apply();
                });
            }
        }
    };
};