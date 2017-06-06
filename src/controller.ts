import * as _ from "lodash";
import { auto, IControllerRegistrations } from "angular";
import { IControllerContainer } from "./angularInterfaces";

"use strict";

export default function $ControllerProvider() {
    var controllers: IControllerContainer = {};
    var globals = false;

    this.allowGlobals = function () {
        globals = true;
    }

    this.register = function (name: string | IControllerRegistrations, controller?: Function) {
        if (_.isObject(name)) {
            _.extend(controllers, name);
        } else if (_.isString(name)) {
            controllers[name] = controller;
        }
    };

    this.$get = ["$injector", function ($injector: auto.IInjectorService) {
        return function (ctrl: Function | string, locals: any) {
            if (_.isString(ctrl)) {
                if (controllers.hasOwnProperty(ctrl)) {
                    ctrl = controllers[ctrl];
                } else if (globals) {
                    ctrl = <Function>window[ctrl];
                } else {
                    throw ctrl + " controller not found.";
                }
            }
            return $injector.instantiate(ctrl, locals);
        };
    }];
}