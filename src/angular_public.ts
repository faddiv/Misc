"use strict";
import { setupModuleLoader } from './loader';
import { default as filter } from './filter';
import { default as parse } from './parse';
import { default as Scope } from './Scope';
import { default as compile } from './compile';
import { default as controller } from './controller';
import { $QProvider, $$QProvider } from './q';
import { $HttpBackendProvider } from './http_backend';
import { $HttpProvider, $HttpParamSerializerProvider, $HttpParamSerializerJQLikeProvider } from './http';
import { default as NgControllerDirective } from './directives/ng_controller';

export function publishExternalAPI() {
    setupModuleLoader(window);

    var ngModule = window.angular.module("ng", []);
    ngModule.provider("$filter", filter);
    ngModule.provider("$parse", parse);
    ngModule.provider("$rootScope", Scope);
    ngModule.provider("$q", $QProvider);
    ngModule.provider("$$q", $$QProvider);
    ngModule.provider("$httpBackend", <any>$HttpBackendProvider);
    ngModule.provider("$http", <any>$HttpProvider);
    ngModule.provider("$httpParamSerializer", <any>$HttpParamSerializerProvider);
    ngModule.provider("$httpParamSerializerJQLike", <any>$HttpParamSerializerJQLikeProvider);
    ngModule.provider("$compile", <any>compile);
    ngModule.provider("$controller", <any>controller);
    ngModule.directive("ngController", NgControllerDirective);
}