"use strict";
import * as _ from "lodash";
import * as sinon from 'sinon';
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { auto, IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService, IScope, IAttributes, IComponentOptions, IControllerProvider } from "angular";
import { IChangesCollection } from "../../src/angularInterfaces";

describe("components", () => {

    var xhr: Sinon.SinonFakeXMLHttpRequest;
    var requests: Sinon.SinonFakeXMLHttpRequest[] = [];

    beforeEach(() => {
        publishExternalAPI();
    });

    afterEach(() => {
        delete window.angular;
    });

    beforeEach(() => {
        xhr = sinon.useFakeXMLHttpRequest();
        xhr.onCreate = function (req) {
            requests.push(req)
        }
    });

    afterEach(() => {
        xhr.restore();
        requests.splice(0, requests.length);
    });

    function makeInjectorWithComponent(options: IComponentOptions): auto.IInjectorService;
    function makeInjectorWithComponent(name: string, options?: IComponentOptions): auto.IInjectorService;
    function makeInjectorWithComponent(name: string | IComponentOptions, options?: IComponentOptions): auto.IInjectorService {
        return createInjector(["ng", function ($compileProvider: ICompileProvider) {
            if (typeof (name) === "string") {
                $compileProvider.component(name, options);
            } else {
                $compileProvider.component("myComponent", name);
            }
        }]);
    }
    //Registering Components
    it("can be registered and became directives", () => {
        var myModule = window.angular.module("myModule", []);
        myModule.component("myComponent", {});
        var injector = createInjector(["ng", "myModule"]);
        expect(injector.has("myComponentDirective")).toBe(true);
    });

    //Basic Components
    it("are element directives with controllers", () => {
        var controllerInstantiated = false;
        var componentElement: JQuery;
        var injector = makeInjectorWithComponent({
            controller($element) {
                controllerInstantiated = true;
                componentElement = $element;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>");
            $compile(el)($rootScope);
            expect(controllerInstantiated).toBe(true);
            expect(el[0]).toBe(componentElement[0]);
        });
    });

    it("cannot be applied to an attribute", () => {
        var controllerInstantiated = false;
        var injector = makeInjectorWithComponent(<any>{
            restrict: "A", //Will be ignorred
            controller() {
                controllerInstantiated = true;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<div my-component></div>");
            $compile(el)($rootScope);
            expect(controllerInstantiated).toBe(false);
        });
    });

    //Component Scopes and Bindings
    it("has an isolate scope", () => {
        var componentScope: IScope;
        var injector = makeInjectorWithComponent({
            controller($scope) {
                componentScope = $scope;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>");
            $compile(el)($rootScope);
            expect(componentScope).not.toBe($rootScope);
            expect(componentScope.$parent).toBe($rootScope);
            expect(Object.getPrototypeOf(componentScope)).not.toBe($rootScope);
        });
    });

    it("may have bindings which are attached to controller", () => {
        var controllerInstance;
        var injector = makeInjectorWithComponent({
            bindings: {
                attr: "@",
                oneWay: "<",
                twoWay: "="
            },
            controller() {
                controllerInstance = this;
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            $rootScope.b = 42;
            $rootScope.c = 43;
            var el = $("<my-component attr='a' one-way='b' two-way='c'></my-component>")
            $compile(el)($rootScope);

            expect(controllerInstance.attr).toEqual("a");
            expect(controllerInstance.oneWay).toEqual(42);
            expect(controllerInstance.twoWay).toEqual(43);
        });
    });

    it("may use a controller alias with controllerAs", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = makeInjectorWithComponent({
            controller($scope) {
                componentScope = $scope;
                controllerInstance = this;
            },
            controllerAs: "ctrl"
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            $rootScope.b = 42;
            $rootScope.c = 43;
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);

            expect(componentScope.ctrl).toEqual(controllerInstance);
        });
    });

    it("may use a controller alias with 'controller as' syntax", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
            $controllerProvider.register("MyController", function ($scope) {
                componentScope = $scope;
                controllerInstance = this;
            });
            $compileProvider.component("myComponent", {
                controller: "MyController as ctrl"
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);

            expect(componentScope.ctrl).toEqual(controllerInstance);
        });
    });

    it("has a default controller alias of $ctrl", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = createInjector(["ng", function ($controllerProvider: IControllerProvider, $compileProvider: ICompileProvider) {
            $controllerProvider.register("MyController", function ($scope) {
                componentScope = $scope;
                controllerInstance = this;
            });
            $compileProvider.component("myComponent", {
                controller: "MyController"
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);

            expect(componentScope.$ctrl).toEqual(controllerInstance);
        });
    });

    //Component Templates
    it("may have template", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = makeInjectorWithComponent({
            controller($scope) {
                this.message = "Hyy";
            },
            template: "{{ $ctrl.message }}"
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);
            $rootScope.$apply();
            expect(el.text()).toEqual("Hyy");
        });
    });

    it("may have a templateUrl", () => {
        var controllerInstance;
        var componentScope: IScope;
        var injector = makeInjectorWithComponent({
            controller($scope) {
                this.message = "Hyy";
            },
            templateUrl: "/my_component.html"
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);
            $rootScope.$apply();
            requests[0].respond(200, {}, "{{ $ctrl.message }}");
            $rootScope.$apply();
            expect(el.text()).toEqual("Hyy");
        });
    });

    it("may have a template function with DI support", () => {
        var injector = createInjector(["ng", function ($provide: auto.IProvideService, $compileProvider: ICompileProvider) {
            $provide.constant("myConstant", 42);
            $compileProvider.component("myComponent", {
                template: function (myConstant) {
                    return "" + myConstant;
                }
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);

            expect(el.text()).toEqual("42");
        });
    });

    it("may have a template function with array-wrapped DI", () => {
        var injector = createInjector(["ng", function ($provide: auto.IProvideService, $compileProvider: ICompileProvider) {
            $provide.constant("myConstant", 42);
            $compileProvider.component("myComponent", {
                template: ["myConstant", function (c) {
                    return "" + c;
                }]
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>");
            $compile(el)($rootScope);

            expect(el.text()).toEqual("42");
        });
    });

    it("may inject $element and attr$ to template function", () => {
        var injector = makeInjectorWithComponent({
            template($element: JQuery, $attrs: IAttributes) {
                return $element.attr("copiedAttr", $attrs.myAttr).html();
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component my-attr='42'></my-component>");
            $compile(el)($rootScope);
            expect(el.attr("copiedAttr")).toEqual("42");
        });
    });


    it("may have a templateUrl function with DI support", () => {
        var injector = createInjector(["ng", function ($provide: auto.IProvideService, $compileProvider: ICompileProvider) {
            $provide.constant("myConstant", "constComponent");
            $compileProvider.component("myComponent", {
                templateUrl: function (myConstant) {
                    return "/" + myConstant + ".html";
                }
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);
            $rootScope.$apply();
            expect(requests[0].url).toEqual("/constComponent.html");
        });
    });

    it("may have a templateUrl function with array-wrapped DI", () => {
        var injector = createInjector(["ng", function ($provide: auto.IProvideService, $compileProvider: ICompileProvider) {
            $provide.constant("myConstant", "constComponent");
            $compileProvider.component("myComponent", {
                templateUrl: ["myConstant", function (c) {
                    return "/" + c + ".html";
                }]
            });
        }])
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component></my-component>")
            $compile(el)($rootScope);
            $rootScope.$apply();
            expect(requests[0].url).toEqual("/constComponent.html");
        });
    });

    it("may inject $element and attr$ to templateUrl function", () => {
        var injector = makeInjectorWithComponent({
            templateUrl($element: JQuery, $attrs: IAttributes) {
                return "/" + $element.prop("tagName").toUpperCase() + $attrs.myAttr + ".html";
            }
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component my-attr='42'></my-component>");
            $compile(el)($rootScope);
            $rootScope.$apply();
            expect(requests[0].url).toEqual("/MY-COMPONENT42.html");
        });
    });

    //Component Transclusion
    it("may use transclusion", () => {
        var injector = makeInjectorWithComponent({
            transclude: true,
            template: "<div ng-transclude></div>"
        });
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<my-component>Transclude me</my-component>");
            $compile(el)($rootScope);
            expect(el.find("div").text()).toEqual("Transclude me");
        });
    });

    //Requiring from Components
    it("may require other directive controllers", () => {
        var secondControllerInstance;
        var val = {};
        var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
            $compileProvider.component("first", {
                controller() {
                    this.val = val;
                }
            });
            $compileProvider.component("second", {
                require: { first: "^" },
                controller() {
                    secondControllerInstance = this;
                }
            });
        }]);
        injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
            var el = $("<first><second></second></first>")
            $compile(el)($rootScope);
            expect(secondControllerInstance.first).toBeDefined();
            expect(secondControllerInstance.first.val).toBe(val);
        });
    });

    //The $onInit Lifecycle Hook
    describe("lifecycle", function () {
        it("calls $onInit after all ctrls created before linking", () => {
            var invocations: string[] = [];
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.component("first", {
                    controller() {
                        invocations.push("first controller created");
                        this.$onInit = function () {
                            invocations.push("first controller $onInit");
                        };
                    }
                });
                $compileProvider.directive({
                    second() {
                        return {
                            controller() {
                                invocations.push("second controller created");
                                this.$onInit = function () {
                                    invocations.push("second controller $onInit");
                                };
                            },
                            link: {
                                pre() {
                                    invocations.push("second prelink");
                                },
                                post() {
                                    invocations.push("second postlink");
                                }
                            }
                        }
                    }
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<first second></first>")
                $compile(el)($rootScope);
                expect(invocations).toEqual([
                    "first controller created",
                    "second controller created",
                    "first controller $onInit",
                    "second controller $onInit",
                    "second prelink",
                    "second postlink"
                ]);
            });
        });

        //The $onDestroy Lifecycle Hook
        it("calls $onDestroy when the scope is destroyed", () => {
            var destroySpy = jasmine.createSpy("destroySpy");
            var injector = makeInjectorWithComponent({
                controller() {
                    this.$onDestroy = destroySpy;
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<my-component></my-component>");
                $compile(el)($rootScope);
                $rootScope.$destroy();
                expect(destroySpy).toHaveBeenCalled();
            });
        });

        //The $postLink Lifecycle Hook
        it("calls $postLink after all linking is done", () => {
            var invocations: string[] = [];
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.component("first", {
                    controller() {
                        this.$postLink = function () {
                            invocations.push("first controller $postLink");
                        };
                    }
                });
                $compileProvider.directive({
                    second() {
                        return {
                            controller() {
                                this.$postLink = function () {
                                    invocations.push("second controller $postLink");
                                };
                            },
                            link: {
                                post() {
                                    invocations.push("second postlink");
                                }
                            }
                        }
                    }
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<first><second></second></first>")
                $compile(el)($rootScope);
                expect(invocations).toEqual([
                    "second postlink",
                    "second controller $postLink",
                    "first controller $postLink"
                ]);
            });
        });

        //The $onChanges Hook
        it("calls $onChanges with all bindings during init", () => {
            var changesSpy = jasmine.createSpy("changesSpy");
            var injector = makeInjectorWithComponent({
                bindings: {
                    myBinding: "<",
                    myAttr: "@"
                },
                controller() {
                    this.$onChanges = changesSpy;
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<my-component my-binding='42' my-attr='43'></my-component>");
                $compile(el)($rootScope);
                expect(changesSpy).toHaveBeenCalled();
                var changes = <IChangesCollection>changesSpy.calls.mostRecent().args[0];
                expect(changes.myBinding.currentValue).toBe(42);
                expect(changes.myBinding.isFirstChange()).toBe(true);
                expect(changes.myAttr.currentValue).toBe("43");
                expect(changes.myAttr.isFirstChange()).toBe(true);
            });
        });

        it("does not call $onChanges for two-way bindings", () => {
            var changesSpy = jasmine.createSpy("changesSpy");
            var injector = makeInjectorWithComponent({
                bindings: {
                    myBinding: "="
                },
                controller() {
                    this.$onChanges = changesSpy;
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<my-component my-binding='42'></my-component>");
                $compile(el)($rootScope);
                expect(changesSpy).toHaveBeenCalled();
                var changes = changesSpy.calls.mostRecent().args[0];
                expect(changes.myBinding).toBeUndefined();
            });
        });

        it("calls $onChanges when binding changes", () => {
            var changesSpy = jasmine.createSpy("changesSpy");
            var injector = makeInjectorWithComponent({
                bindings: {
                    myBinding: "<"
                },
                controller() {
                    this.$onChanges = changesSpy;
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                $rootScope.aValue = 42;
                var el = $("<my-component my-binding='aValue'></my-component>");
                $compile(el)($rootScope);
                $rootScope.$apply();
                expect(changesSpy.calls.count()).toBe(1);

                $rootScope.aValue = 43;
                $rootScope.$apply();
                expect(changesSpy.calls.count()).toBe(2);
                var changes = <IChangesCollection>changesSpy.calls.mostRecent().args[0];
                expect(changes.myBinding.currentValue).toBe(43);
                expect(changes.myBinding.previousValue).toBe(42);
                expect(changes.myBinding.isFirstChange()).toBe(false);
            });
        });

        it("calls $onChanges when attribute changes", () => {
            var changesSpy = jasmine.createSpy("changesSpy");
            var attrs: IAttributes;
            var injector = makeInjectorWithComponent({
                bindings: {
                    myAttr: "@"
                },
                controller($attrs) {
                    this.$onChanges = changesSpy;
                    attrs = $attrs;
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                $rootScope.aValue = 42;
                var el = $("<my-component my-attr='42'></my-component>");
                $compile(el)($rootScope);
                $rootScope.$apply();
                expect(changesSpy.calls.count()).toBe(1);

                attrs.$set("myAttr", "43");
                $rootScope.$apply();
                expect(changesSpy.calls.count()).toBe(2);
                var changes = <IChangesCollection>changesSpy.calls.mostRecent().args[0];
                expect(changes.myAttr.currentValue).toBe("43");
                expect(changes.myAttr.previousValue).toBe("42");
                expect(changes.myAttr.isFirstChange()).toBe(false);
            });
        });

        it("calls $onChanges once with multiple changes", () => {
            var changesSpy = jasmine.createSpy("changesSpy");
            var attrs: IAttributes;
            var injector = makeInjectorWithComponent({
                bindings: {
                    myBinding: "<",
                    myAttr: "@"
                },
                controller($attrs) {
                    this.$onChanges = changesSpy;
                    attrs = $attrs;
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                $rootScope.aValue = 42;
                var el = $("<my-component my-binding='aValue' my-attr='fortytwo'></my-component>");
                $compile(el)($rootScope);
                $rootScope.$apply();
                expect(changesSpy.calls.count()).toBe(1);

                $rootScope.aValue = 43;
                attrs.$set("myAttr", "fortythree");
                $rootScope.$apply();
                expect(changesSpy.calls.count()).toBe(2);
                var changes = <IChangesCollection>changesSpy.calls.mostRecent().args[0];
                expect(changes.myBinding.currentValue).toBe(43);
                expect(changes.myBinding.previousValue).toBe(42);
                expect(changes.myAttr.currentValue).toBe("fortythree");
                expect(changes.myAttr.previousValue).toBe("fortytwo");
            });

        });

        it("runs $onChanges in a digest", () => {
            var injector = makeInjectorWithComponent({
                bindings: {
                    myBinding: "<"
                },
                controller() {
                    this.$onChanges = function () {
                        this.innerValue = "myBinding is " + this.myBinding;
                    };
                },
                template: "{{$ctrl.innerValue}}"
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                $rootScope.aValue = 42;
                var el = $("<my-component my-binding='aValue'></my-component>");
                $compile(el)($rootScope);
                $rootScope.$apply();
                $rootScope.aValue = 43;
                $rootScope.$apply();

                expect(el.text()).toEqual("myBinding is 43");
            });
        });

        it("keeps first value as previous for $onChanges when multiple changes", () => {
            var changesSpy = jasmine.createSpy("changesSpy");
            var injector = makeInjectorWithComponent({
                bindings: {
                    myBinding: "<"
                },
                controller() {
                    this.$onChanges = changesSpy;
                },
                template: "{{$ctrl.innerValue}}"
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                $rootScope.aValue = 42;
                var el = $("<my-component my-binding='aValue'></my-component>");
                $compile(el)($rootScope);
                $rootScope.$apply();

                $rootScope.aValue = 43;
                $rootScope.$watch("aValue", function () {
                    if ($rootScope.aValue !== 44) {
                        $rootScope.aValue = 44;
                    }
                });
                $rootScope.$apply();
                expect(changesSpy.calls.count()).toBe(2);

                var changes = <IChangesCollection>changesSpy.calls.mostRecent().args[0];
                expect(changes.myBinding.currentValue).toBe(44);
                expect(changes.myBinding.previousValue).toBe(42);
            });
        });

        it("runs $onChanges for all components in the same digest", () => {
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.component("first", {
                    bindings: {
                        myBinding: "<"
                    },
                    controller() {
                        this.$onChanges = function () { };
                    }
                });
                $compileProvider.component("second", {
                    bindings: {
                        myBinding: "<"
                    },
                    controller() {
                        this.$onChanges = function () { };
                    }
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var watchSpy = jasmine.createSpy("watchSpy");
                $rootScope.$watch(watchSpy);
                $rootScope.aValue = 42;
                var el = $("<div><first my-binding='aValue'></first><second my-binding='aValue'></second></div>")
                $compile(el)($rootScope);
                $rootScope.$apply();
                // Dirty watches always cause a second digest, hence 2
                expect(watchSpy.calls.count()).toBe(2);

                $rootScope.aValue = 43;
                $rootScope.$apply();
                // Two more because of dirty watches,
                // plus *one* more for onchanges
                expect(watchSpy.calls.count()).toBe(5);
            });
        });

        it("has a TTL for $onChanges", () => {
            var injector = makeInjectorWithComponent({
                bindings: {
                    input: "<",
                    increment: "="
                },
                controller() {
                    this.$onChanges = function () {
                        if (this.increment) {
                            this.increment = this.increment + 1;
                        }
                    };
                }
            });
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div>" +
                    "<my-component input='valueOne' increment='valueTwo'></my-component>" +
                    "<my-component input='valueTwo' increment='valueOne'></my-component>" +
                    "</div>")
                $compile(el)($rootScope);
                $rootScope.$apply();

                $rootScope.valueOne = 42;
                $rootScope.valueTwo = 42;
                $rootScope.$apply();
                expect($rootScope.valueOne).toBe(51);

                expect($rootScope.valueTwo).toBe(51);
            });
        }, 10);

        it("allows configuring $onChanges TTL", () => {
            var injector = createInjector(["ng", function ($compileProvider: ICompileProvider) {
                $compileProvider.onChangesTtl(50);
                $compileProvider.component("myComponent", {
                    bindings: {
                        input: "<",
                        increment: "="
                    },
                    controller() {
                        this.$onChanges = function () {
                            if (this.increment) {
                                this.increment = this.increment + 1;
                            }
                        };
                    }
                });
            }]);
            injector.invoke(function ($compile: ICompileService, $rootScope: IScope) {
                var el = $("<div>" +
                    "<my-component input='valueOne' increment='valueTwo'></my-component>" +
                    "<my-component input='valueTwo' increment='valueOne'></my-component>" +
                    "</div>")
                $compile(el)($rootScope);
                $rootScope.$apply();

                $rootScope.valueOne = 42;
                $rootScope.valueTwo = 42;
                $rootScope.$apply();
                expect($rootScope.valueOne).toBe(91);

                expect($rootScope.valueTwo).toBe(91);
            });
        }, 10);

    });
});