"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes, IControllerProvider, auto } from "angular";
import { IScopeEx } from "../../typings/testInterfaces";

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

    function makeInjectorWithDirectives(name: any, ...params: object[]) {
        var args = arguments;
        return createInjector(["ng", function ($compileProvider: ICompileProvider) {
            $compileProvider.directive.apply($compileProvider, args);
        }]);
    }

    //Directive Controllers
    describe("controllers", () => {

        it("can be attached to directives as functions", () => {
            var controllerInvoked;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    controller: function MyController() {
                        controllerInvoked = true;
                    }
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(controllerInvoked).toBe(true);
            });
        });

        it("can be attached to directives as string references", () => {
            var controllerInvoked;
            function MyController() {
                controllerInvoked = true;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(controllerInvoked).toBe(true);
            });
        });

        it("can be applied in the same element independent of each other", () => {
            var controllerInvoked;
            var otherControllerInvoked;
            function MyController() {
                controllerInvoked = true;
            }
            function MyOtherController() {
                otherControllerInvoked = true;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $controllerProvider.register("MyOtherController", MyOtherController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                    $compileProvider.directive("myOtherDirective", function () {
                        return {
                            controller: "MyOtherController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(controllerInvoked).toBe(true);
                expect(otherControllerInvoked).toBe(true);
            });
        });

        it("can be applied to different directives, as different instances", () => {
            var invocations = 0;;
            function MyController() {
                invocations++;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                    $compileProvider.directive("myOtherDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(invocations).toBe(2);
            });
        });

        it("can be aliased with @ when given in directive attribute", () => {
            var controllerInvoked;
            function MyController() {
                controllerInvoked = true;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "@"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='MyController'>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(controllerInvoked).toBe(true);
            });
        });
        //Locals in Directive Controllers
        it("gets scope, element, and attrs through DI", () => {
            var gotScope: IScope, gotElement: JQuery, gotAttrs: IAttributes;
            function MyController($element: JQuery, $scope: IScope, $attrs: IAttributes) {
                gotElement = $element;
                gotScope = $scope;
                gotAttrs = $attrs;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive an-attr='abc'>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotElement[0]).toBe(el[0]);
                expect(gotScope).toBe($rootScope);
                expect(gotAttrs).toBeDefined();
                expect(gotAttrs.anAttr).toEqual("abc");
            });
        });
        //Attaching Directive Controllers on The Scope
        it("can be attached on the scope", () => {
            function MyController() { }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            controller: "MyController",
                            controllerAs: "ctrl"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect($rootScope.ctrl).toBeDefined();
                expect($rootScope.ctrl instanceof MyController).toBe(true);
            });
        });
        //Controllers on Isolate Scope Directives
        it("gets isolated scope as injected $scope", () => {
            var gotScope: IScope;
            function MyController($scope: IScope) {
                gotScope = $scope;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            scope: {},
                            controller: "MyController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotScope).not.toBe($rootScope);
                expect(gotScope.$parent).toBe($rootScope);
            });
        });

        it("has isolate scope bindings available during construction", () => {
            var gotMyAttr;
            function MyController($scope: IScope) {
                gotMyAttr = $scope.myAttr;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            scope: {
                                myAttr: "@myDirective"
                            },
                            controller: "MyController"
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='abc'>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotMyAttr).toEqual("abc");
            });
        });

        it("can bind isolate scope bindings directly to self", () => {
            var gotMyAttr;
            function MyController($scope: IScope) {
                gotMyAttr = this.myAttr;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            scope: {
                                myAttr: "@myDirective"
                            },
                            controller: "MyController",
                            bindToController: true
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='abc'>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotMyAttr).toEqual("abc");
            });
        });

        it("can bind isolate scope bindings through bindToController", () => {
            var gotMyAttr;
            function MyController() {
                gotMyAttr = this.myAttr;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            scope: {},
                            controller: "MyController",
                            bindToController: {
                                myAttr: "@myDirective"
                            }
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='abc'>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotMyAttr).toEqual("abc");
            });
        });

        it("can bind through bindToController without isolate scope", () => {
            var gotMyAttr;
            function MyController() {
                gotMyAttr = this.myAttr;
            }
            var injector = createInjector(["ng",
                function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
                    $controllerProvider.register("MyController", MyController);
                    $compileProvider.directive("myDirective", function () {
                        return {
                            scope: true,
                            controller: "MyController",
                            bindToController: {
                                myAttr: "@myDirective"
                            }
                        };
                    });
                }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive='abc'>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotMyAttr).toEqual("abc");
            });
        });
        //Requiring Controllers
        it("can be required from a sibling directive", () => {
            function MyController() { }
            var gotCtrl;
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.directive("myDirective", function () {
                    return {
                        scope: {},
                        controller: MyController
                    };
                });
                $compileProvider.directive("myOtherDirective", function () {
                    return {
                        require: "myDirective",
                        link(scope, element, attrs, ctrl) {
                            gotCtrl = ctrl;
                        }
                    };
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotCtrl).toBeDefined();
                expect(gotCtrl instanceof MyController).toBe(true);
            });
        });
        //Requiring Multiple Controllers
        it("can be required from multiple sibling directives", () => {
            function MyController() { }
            function MyOtherController() { }
            var gotCtrls: any[];
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.directive("myDirective", function () {
                    return {
                        scope: true,
                        controller: MyController
                    };
                });
                $compileProvider.directive("myOtherDirective", function () {
                    return {
                        scope: true,
                        controller: MyOtherController
                    };
                });
                $compileProvider.directive("myThirdDirective", function () {
                    return {
                        require: ["myDirective", "myOtherDirective"],
                        link(scope, element, attrs, ctrls) {
                            if (_.isArray(ctrls)) {
                                gotCtrls = ctrls;
                            }
                        }
                    };
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive my-third-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotCtrls).toBeDefined();
                expect(gotCtrls.length).toBe(2);
                expect(gotCtrls[0] instanceof MyController).toBe(true);
                expect(gotCtrls[1] instanceof MyOtherController).toBe(true);
            });
        });
        //Requiring Multiple Controllers as an Object
        it("can be required as an object", () => {
            function MyController() { }
            function MyOtherController() { }
            var gotCtrls: any;
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.directive("myDirective", function () {
                    return {
                        scope: true,
                        controller: MyController
                    };
                });
                $compileProvider.directive("myOtherDirective", function () {
                    return {
                        scope: true,
                        controller: MyOtherController
                    };
                });
                $compileProvider.directive("myThirdDirective", function () {
                    return {
                        require: {
                            myDirective: "myDirective",
                            myOtherDirective: "myOtherDirective"
                        },
                        link(scope, element, attrs, ctrls) {
                            gotCtrls = ctrls;
                        }
                    };
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive my-third-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotCtrls).toBeDefined();
                expect(gotCtrls.myDirective instanceof MyController).toBe(true);
                expect(gotCtrls.myOtherDirective instanceof MyOtherController).toBe(true);
            });
        });
        //Requiring Multiple Controllers as an Object
        it("can be required as an object with values omitted", () => {
            function MyController() { }
            function MyOtherController() { }
            var gotCtrls: any;
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.directive("myDirective", function () {
                    return {
                        scope: true,
                        controller: MyController
                    };
                });
                $compileProvider.directive("myOtherDirective", function () {
                    return {
                        scope: true,
                        controller: MyOtherController
                    };
                });
                $compileProvider.directive("myThirdDirective", function () {
                    return {
                        require: {
                            myDirective: "",
                            myOtherDirective: ""
                        },
                        link(scope, element, attrs, ctrls) {
                            gotCtrls = ctrls;
                        }
                    };
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div my-directive my-other-directive my-third-directive>");
                var linkFn = $compile(el);
                linkFn($rootScope);
                expect(gotCtrls).toBeDefined();
                expect(gotCtrls.myDirective instanceof MyController).toBe(true);
                expect(gotCtrls.myOtherDirective instanceof MyOtherController).toBe(true);
            });
        });
        //Self-Requiring Directives
    });
});