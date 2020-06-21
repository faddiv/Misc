"use strict";

export default function ngControllerDirective() {
    return {
        restrict: "A",
        scope: true,
        controller: "@"
    };
};