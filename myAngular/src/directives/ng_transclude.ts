import { IScope, IAttributes, ITranscludeFunction } from "angular";

"use strict";

export default function NgTranscludeDirective() {
    return {
        restrict: "EAC",
        link(scope: IScope, element: JQuery, attrs: IAttributes, ctrl: any, transclude: ITranscludeFunction) {
            transclude(function (clone: JQuery) {
                element.empty();
                element.append(clone);
            })
        }
    };
};