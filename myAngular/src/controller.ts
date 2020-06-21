import * as _ from "lodash";
import { auto, IControllerRegistrations, IControllerLocals } from "angular";
import { IControllerContainer, ILateBoundController } from "./angularInterfaces";

"use strict";
const CNTRL_REG = /^(\S+)(\s+as\s+(\w+))?/;
function addToScope(locals: IControllerLocals, identifier: string, instance: any) {
    if (locals && _.isObject(locals.$scope)) {
        locals.$scope[identifier] = instance;
    } else {
        throw "Cannot export controller as " + identifier + "! No $scope object provided via locals";
    }
}
export function identifierForController(ctrl: string | IControllerRegistrations) {
    if (_.isString(ctrl)) {
        var match = ctrl.match(CNTRL_REG);
        ctrl = match[1];
        return match[3];
    }
}

export default function $ControllerProvider() {
    var controllers: IControllerContainer = {};
    var globals = false;

    this.allowGlobals = function () {
        globals = true;
    }

    this.register = function (name: string | IControllerRegistrations, controller?: ILateBoundController<any>) {
        if (_.isObject(name)) {
            _.extend(controllers, name);
        } else if (_.isString(name)) {
            controllers[name] = controller;
        }
    };

    this.$get = ["$injector", function ($injector: auto.IInjectorService) {
        return function (ctrl: Function | string | Function[], locals: any, later: boolean, identifier: string) {
            if (_.isString(ctrl)) {
                var match = ctrl.match(CNTRL_REG);
                ctrl = match[1];
                identifier = identifier || match[3];
                if (controllers.hasOwnProperty(ctrl)) {
                    ctrl = controllers[ctrl];
                } else {
                    var ctrl2: Function = (locals && locals.$scope && locals.$scope[ctrl]) ||
                        (globals && <Function>window[ctrl]);
                    if (!ctrl) {
                        throw ctrl + " controller not found.";
                    }
                    ctrl = ctrl2;
                }
            }
            var instance: any;
            if (later) {
                var ctrlConstructor = _.isArray<Function>(ctrl) ? _.last<Function>(ctrl) : ctrl;
                instance = Object.create(ctrlConstructor.prototype);
                if (identifier) {
                    addToScope(locals, identifier, instance);
                }
                return _.extend(function () {
                    $injector.invoke(<Function>ctrl, instance, locals);
                    return instance;
                }, {
                        instance: instance
                    });
            } else {
                instance = $injector.instantiate<any>(ctrl, locals);
                if (identifier) {
                    addToScope(locals, identifier, instance);
                }
            }
            return instance;
        };
    }];
}