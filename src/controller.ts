import * as _ from "lodash";
import { auto, IControllerRegistrations, IControllerLocals } from "angular";
import { IControllerContainer, ILateBoundController } from "./angularInterfaces";

"use strict";

function addToScope(locals: IControllerLocals, identifier: string, instance: any) {
    if (locals && _.isObject(locals.$scope)) {
        locals.$scope[identifier] = instance;
    } else {
        throw "Cannot export controller as " + identifier + "! No $scope object provided via locals";
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
        return function (ctrl: Function | string, locals: any, later: boolean, identifier: string) {
            if (_.isString(ctrl)) {
                if (controllers.hasOwnProperty(ctrl)) {
                    ctrl = controllers[ctrl];
                } else if (globals) {
                    ctrl = <Function>window[ctrl];
                } else {
                    throw ctrl + " controller not found.";
                }
            }
            var instance: any;
            if (later) {
                var ctrlConstructor = _.isArray<Function>(ctrl) ? _.last<Function>(ctrl) : ctrl;
                instance = Object.create(ctrlConstructor.prototype);
                if(identifier) {
                    addToScope(locals, identifier, instance);
                }
                return _.extend(function() { 
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