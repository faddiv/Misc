import * as _ from "lodash";
import { ICompileProvider, IDirectiveFactory, auto, Injectable, IDirective } from "angular";

"use strict";

const PREFIX_REGEXP = /(x[\:\-_]|data[\:\-_])/i;

function nodeName(element: HTMLElement | HTMLElement[]): string {
    return _.isArray(element) ? element[0].nodeName : element.nodeName;
}

function directiveNormalize(name: string) {
    return _.camelCase(name.replace(PREFIX_REGEXP, ""));
}

export default function $CompileProvider($provide: auto.IProvideService) {

    var hasDirectives: any = {};

    this.directive = function (name: string | { [directiveName: string]: Injectable<IDirectiveFactory> }, directiveFactory?: IDirectiveFactory) {
        if (_.isString(name)) {
            if (name === "hasOwnProperty") {
                throw "hasOwnProperty is not a valid directive name";
            }
            if (!hasDirectives.hasOwnProperty(name)) {
                hasDirectives[name] = [];
                $provide.factory(name + "Directive", ["$injector", function ($injector: auto.IInjectorService) {
                    var factories = hasDirectives[name];
                    return _.map(factories, function (factory: any) {
                        var directive = $injector.invoke(factory);
                        directive.restrict = directive.restrict || "EA";
                        return directive;
                    });
                }]);
            }
            hasDirectives[name].push(directiveFactory);
        } else {
            _.forEach(name, (directiveFactory, name) => {
                this.directive(name, directiveFactory);
            });
        }
    };

    this.$get = ["$injector", function ($injector: auto.IInjectorService) {
        function compile($compileNodes: JQuery) {
            return compileNodes($compileNodes);
        }

        function compileNodes($compileNodes: JQuery) {
            _.forEach($compileNodes, function (node: HTMLElement) {
                var directives = collectDirectives(node);
                applyDirectivesToNode(directives, node);
                if (node.childNodes && node.childNodes.length) {
                    compileNodes(<any>node.childNodes);
                }
            });
        }

        function collectDirectives(node: HTMLElement): IDirective[] {
            var directives: IDirective[] = [];
            if (node.nodeType === Node.ELEMENT_NODE) {
                var normalizedNodeName = directiveNormalize(nodeName(node).toLocaleLowerCase());
                addDirective(directives, normalizedNodeName, "E");
                _.forEach(node.attributes, function (attr) {
                    var normalizedAttrName = directiveNormalize(attr.name.toLowerCase());
                    if (/^ngAttr[A-Z]/.test(normalizedAttrName)) {
                        normalizedAttrName =
                            normalizedAttrName[6].toLocaleLowerCase() +
                            normalizedAttrName.substring(7);
                    }
                    addDirective(directives, normalizedAttrName, "A");
                });
                _.forEach(node.classList, function (cls) {
                    var normalizedClassName = directiveNormalize(cls);
                    addDirective(directives, normalizedClassName, "C");
                });
            } else if (node.nodeType === Node.COMMENT_NODE) {
                var match = /^\s*directive\:\s*([\d\w\-_]+)/.exec(node.nodeValue);
                if (match) {
                    addDirective(directives, directiveNormalize(match[1]), "M");
                }
            }
            return directives;
        }

        function addDirective(directives: IDirective[], name: string, mode: string) {
            if (hasDirectives.hasOwnProperty(name)) {
                var foundDirectives = $injector.get<IDirective[]>(name + "Directive");
                var applicableDirectives = _.filter(foundDirectives, function (dir) {
                    return dir.restrict.indexOf(mode) !== -1;
                });
                directives.push.apply(directives, applicableDirectives);
            }
        }

        function applyDirectivesToNode(directives: IDirective[], compileNode: HTMLElement) {
            var $compileNode = $(compileNode);
            _.forEach(directives, function (directive) {
                if (directive.compile) {
                    directive.compile($compileNode, undefined, undefined);
                }
            });
        }

        return compile;
    }];
}

$CompileProvider.$inject = ["$provide"];