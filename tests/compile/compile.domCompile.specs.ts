"use strict";
import * as _ from "lodash";
import { publishExternalAPI } from '../../src/angular_public';
import { createInjector } from '../../src/injector';
import { IAngularStatic, IDirectiveFactory, IModule, ICompileProvider, ICompileService } from "angular";
import $ from "jquery";

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

    it("compiles element directives from a single element", () => {
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    element.data("hasCompiled", true);
                }
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<my-directive></my-directive>");
            $compile(el);
            expect(el.data("hasCompiled")).toBe(true);
        });
    });

    it("compiles element directives found from several elements", () => {
        var idx = 1;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    element.data("hasCompiled", idx++);
                }
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<my-directive></my-directive><my-directive></my-directive>");
            $compile(el);
            expect(el.eq(0).data("hasCompiled")).toBe(1);
            expect(el.eq(1).data("hasCompiled")).toBe(2);
        })
    });
    //Recursing to Child Elements
    it("compiles element directives from child elements", () => {
        var idx = 1;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    element.data("hasCompiled", idx++);
                }
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div><my-directive></my-directive></div>");
            $compile(el);
            expect(el.data("hasCompiled")).toBeUndefined();
            expect(el.find("> my-directive").data("hasCompiled")).toBe(1);
        });
    });

    it("compiles nested directives", () => {
        var idx = 1;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    element.data("hasCompiled", idx++);
                }
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<my-directive><my-directive><my-directive></my-directive></my-directive></my-directive>");
            $compile(el);
            expect(el.data("hasCompiled")).toBe(1);
            expect(el.find("> my-directive").data("hasCompiled")).toBe(2);
            expect(el.find("> my-directive > my-directive").data("hasCompiled")).toBe(3);
        });
    });
    //Using Prefixes with Element Directives
    _.forEach(["x", "data"], function (prefix: string) {
        _.forEach([":", "-", "_"], function (delim: string) {
            it("compiles element directives with " + prefix + delim + " prefix", () => {
                var injector = makeInjectorWithDirectives("myDirective", function () {
                    return {
                        restrict: 'EACM',
                        compile(element: JQuery) {
                            element.data("hasCompiled", true);
                        }
                    };
                });
                injector.invoke(function ($compile: ICompileService) {
                    var el = $("<" + prefix + delim + "my-directive><" + prefix + delim + "/my-directive>");
                    $compile(el);
                    expect(el.data("hasCompiled")).toBe(true);
                });
            });
            //Applying Directives to Attributes
            it("compiles attribute directives with " + prefix + delim + " prefix", () => {
                var injector = makeInjectorWithDirectives("myDirective", function () {
                    return {
                        restrict: 'EACM',
                        compile(element: JQuery) {
                            element.data("hasCompiled", true);
                        }
                    };
                });
                injector.invoke(function ($compile: ICompileService) {
                    var el = $("<div " + prefix + delim + "my-directive></div>");
                    $compile(el);
                    expect(el.data("hasCompiled")).toBe(true);
                });
            });

            it("compiles attribute directives with " + prefix + delim + "ng-attr- prefix", () => {
                var injector = makeInjectorWithDirectives("myDirective", function () {
                    return {
                        restrict: 'EACM',
                        compile(element: JQuery) {
                            element.data("hasCompiled", true);
                        }
                    };
                });
                injector.invoke(function ($compile: ICompileService) {
                    var el = $("<div " + prefix + delim + "ng-attr-my-directive></div>");
                    $compile(el);
                    expect(el.data("hasCompiled")).toBe(true);
                });
            });
            //Applying Directives to Classes
            it("compiles class directives with " + prefix + delim + " prefix", () => {
                var injector = makeInjectorWithDirectives("myDirective", function () {
                    return {
                        restrict: 'EACM',
                        compile(element: JQuery) {
                            element.data("hasCompiled", true);
                        }
                    };
                });
                injector.invoke(function ($compile: ICompileService) {
                    var el = $('<div class="' + prefix + delim + 'my-directive"></div>');
                    $compile(el);
                    expect(el.data("hasCompiled")).toBe(true);
                });
            });
        });
    });
    //Applying Directives to Attributes
    it("compiles attribute directives", () => {
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    element.data("hasCompiled", true);
                }
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive></div>");
            $compile(el);
            expect(el.data("hasCompiled")).toBe(true);
        });
    });

    it("compiles several attribute directives in an element", () => {
        var injector = makeInjectorWithDirectives({
            "myDirective": function () {
                return {
                    restrict: 'EACM',
                    compile(element: JQuery) {
                        element.data("hasCompiled", true);
                    }
                };
            },
            "mySecondDirective": function () {
                return {
                    restrict: 'EACM',
                    compile(element: JQuery) {
                        element.data("secondCompiled", true);
                    }
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div my-directive my-second-directive></div>");
            $compile(el);
            expect(el.data("hasCompiled")).toBe(true);
            expect(el.data("secondCompiled")).toBe(true);
        });
    });

    it("compiles both element and attributes directives in an element", () => {
        var injector = makeInjectorWithDirectives({
            "myDirective": function () {
                return {
                    restrict: 'EACM',
                    compile(element: JQuery) {
                        element.data("hasCompiled", true);
                    }
                };
            },
            "mySecondDirective": function () {
                return {
                    restrict: 'EACM',
                    compile(element: JQuery) {
                        element.data("secondCompiled", true);
                    }
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<my-directive my-second-directive></my-directive>");
            $compile(el);
            expect(el.data("hasCompiled")).toBe(true);
            expect(el.data("secondCompiled")).toBe(true);
        });
    });

    it("compiles attribute directives with ng-attr- prefix", () => {
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    element.data("hasCompiled", true);
                }
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $("<div ng-attr-my-directive></div>");
            $compile(el);
            expect(el.data("hasCompiled")).toBe(true);
        });
    });
    //Applying Directives to Classes
    it("compiles class directives", () => {
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    element.data("hasCompiled", true);
                }
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $('<div class="my-directive"></div>');
            $compile(el);
            expect(el.data("hasCompiled")).toBe(true);
        });
    });

    it("compiles several class directives in an element", () => {
        var injector = makeInjectorWithDirectives({
            "myDirective": function () {
                return {
                    restrict: 'EACM',
                    compile(element: JQuery) {
                        element.data("hasCompiled", true);
                    }
                };
            },
            "mySecondDirective": function () {
                return {
                    restrict: 'EACM',
                    compile(element: JQuery) {
                        element.data("secondCompiled", true);
                    }
                };
            }
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $('<div class="my-directive my-second-directive"></div>');
            $compile(el);
            expect(el.data("hasCompiled")).toBe(true);
            expect(el.data("secondCompiled")).toBe(true);
        });
    });
    //Applying Directives to Comments
    it("compiles comment directives", () => {
        var hasCompiled = false;
        var injector = makeInjectorWithDirectives("myDirective", function () {
            return {
                restrict: 'EACM',
                compile(element: JQuery) {
                    hasCompiled = true;
                }
            };
        });
        injector.invoke(function ($compile: ICompileService) {
            var el = $('<!-- directive: my-directive -->');
            $compile(el);
            expect(hasCompiled).toBe(true);
        });
    });
    //Restricting Directive Application
    _.forEach({
        E: { element: true, attribute: false, class: false, comment: false },
        A: { element: false, attribute: true, class: false, comment: false },
        C: { element: false, attribute: false, class: true, comment: false },
        M: { element: false, attribute: false, class: false, comment: true },
        EA: { element: true, attribute: true, class: false, comment: false },
        AC: { element: false, attribute: true, class: true, comment: false },
        EAM: { element: true, attribute: true, class: false, comment: true },
        EACM: { element: true, attribute: true, class: true, comment: true },
    }, function (expected, restrict) {
        describe("restricted to " + restrict, () => {
            _.forEach({
                element: '<my-directive></my-directive>',
                attribute: '<div my-directive></div>',
                class: '<div class="my-directive"></div>',
                comment: '<!-- directive: my-directive -->'
            }, function (dom, type) {
                it((expected[type] ? "matches" : "does not match") + " on " + type, () => {
                    var hasCompiled = false;
                    var injector = makeInjectorWithDirectives("myDirective", function () {
                        return {
                            restrict: restrict,
                            compile(element: JQuery) {
                                hasCompiled = true;
                            }
                        };
                    });
                    injector.invoke(function ($compile: ICompileService) {
                        var el = $(dom);
                        $compile(el);
                        expect(hasCompiled).toBe(expected[type]);
                    });
                });
            });
        });
    });

    _.forEach({
        element: { dom: '<my-directive></my-directive>', apply: true },
        attribute: { dom: '<div my-directive></div>', apply: true },
        class: { dom: '<div class="my-directive"></div>', apply: false },
        comment: { dom: '<!-- directive: my-directive -->', apply: false }
    }, function (item, type) {
        it((item.apply ? "applies" : "does not apply") + " to " + type + " when no restrict given", () => {
            var hasCompiled = false;
            var injector = makeInjectorWithDirectives("myDirective", function () {
                return {
                    compile(element: JQuery) {
                        hasCompiled = true;
                    }
                };
            });
            injector.invoke(function ($compile: ICompileService) {
                var el = $(item.dom);
                $compile(el);
                expect(hasCompiled).toBe(item.apply);
            });
        });
    });

    //Prioritizing Directives
});