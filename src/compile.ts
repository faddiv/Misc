import * as _ from "lodash";
import { ICompileProvider, IDirectiveFactory, auto, Injectable, IDirective, IAttributes } from "angular";
import { IDirectiveInternal } from "./angularInterfaces";

"use strict";

const PREFIX_REGEXP = /(x[\:\-_]|data[\:\-_])/i;
const BOOLEAN_ATTRS = {
    multiple: true,
    selected: true,
    checked: true,
    disabled: true,
    readOnly: true,
    required: true,
    open: true
};

const BOOLEAN_ELEMENTS = {
    INPUT: true,
    SELECT: true,
    OPTION: true,
    TEXTAREA: true,
    BUTTON: true,
    DETAILS: true
};

function nodeName(element: HTMLElement | HTMLElement[]): string {
    return _.isArray(element) ? element[0].nodeName : element.nodeName;
}

function directiveNormalize(name: string) {
    return _.camelCase(name.replace(PREFIX_REGEXP, ""));
}

function isBooleanAttribute(node: Element, attrName: string) {
    return BOOLEAN_ATTRS[attrName] && BOOLEAN_ELEMENTS[node.nodeName];
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
                    return _.map(factories, function (factory: any, i: number) {
                        var directive: IDirectiveInternal = $injector.invoke(factory);
                        directive.restrict = directive.restrict || "EA";
                        directive.priority = directive.priority || 0;
                        directive.name = directive.name || name;
                        directive.index = i;
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
        class Attributes implements IAttributes {
            $attr: Object
            constructor(private $$element: JQuery) {
                this.$attr = {};
            }
            $normalize(name: string): string { return undefined; }
            $addClass(classVal: string): void { return undefined; }
            $removeClass(classVal: string): void { return undefined; }
            $updateClass(newClasses: string, oldClasses: string): void { return undefined; }
            $set(key: string, value: any, writeAttr?: boolean, attrName?: string): void {
                this[key] = value;
                if (isBooleanAttribute(this.$$element[0], key)) {
                    this.$$element.prop(key, value);
                }

                if (!attrName) {
                    if (this.$attr[key]) {
                        attrName = this.$attr[key];
                    } else {
                        attrName = this.$attr[key] = _.kebabCase(key);
                    }
                } else {
                    this.$attr[key] = attrName;
                }

                if (writeAttr !== false) {
                    this.$$element.attr(attrName, value);
                }
            }
            $observe<T>(name: string, fn: (value?: T) => any): Function { return undefined; }
        }
        function Attributes2(element: Element) {
            this.$$element = element;
        }
        function compile($compileNodes: JQuery) {
            return compileNodes($compileNodes);
        }

        function compileNodes($compileNodes: JQuery) {
            _.forEach($compileNodes, function (node: HTMLElement) {
                var attrs = new Attributes($(node));
                var directives = collectDirectives(node, attrs);
                var terminal = applyDirectivesToNode(directives, node, attrs);
                if (!terminal && node.childNodes && node.childNodes.length) {
                    compileNodes(<any>node.childNodes);
                }
            });
        }

        function collectDirectives(node: HTMLElement, attrs: IAttributes): IDirectiveInternal[] {
            var directives: IDirectiveInternal[] = [];
            if (node.nodeType === Node.ELEMENT_NODE) {
                var normalizedNodeName = directiveNormalize(nodeName(node).toLocaleLowerCase());
                addDirective(directives, normalizedNodeName, "E");
                _.forEach(node.attributes, function (attr) {
                    var attrStartName: string;
                    var attrEndName: string;
                    var name = attr.name;
                    var normalizedAttrName = directiveNormalize(attr.name.toLowerCase());
                    var isNgAttr = /^ngAttr[A-Z]/.test(normalizedAttrName);
                    if (isNgAttr) {
                        name = _.kebabCase(
                            normalizedAttrName[6].toLocaleLowerCase() +
                            normalizedAttrName.substring(7)
                        );
                        normalizedAttrName = directiveNormalize(name.toLowerCase());
                    }
                    attrs.$attr[normalizedAttrName] = name;
                    var directiveNName = normalizedAttrName.replace(/(Start|End)$/, "");
                    if (directiveIsMultiElement(directiveNName)) {
                        if (/Start$/.test(normalizedAttrName)) {
                            attrStartName = name;
                            attrEndName = name.substring(0, name.length - 5) + "end";
                            name = name.substring(0, name.length - 6);
                        }
                    }
                    normalizedAttrName = directiveNormalize(name.toLowerCase());
                    addDirective(directives, normalizedAttrName, "A", attrStartName, attrEndName);
                    if (isNgAttr || !attrs.hasOwnProperty(normalizedAttrName)) {
                        attrs[normalizedAttrName] = attr.value.trim();
                        if (isBooleanAttribute(node, normalizedAttrName)) {
                            attrs[normalizedAttrName] = true;
                        }
                    }
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
            directives.sort(byPriority);
            return directives;
        }
        function directiveIsMultiElement(name: string) {
            if (hasDirectives.hasOwnProperty(name)) {
                var directives = $injector.get(name + "Directive");
                return _.some(directives, { multiElement: true });
            }
            return false;
        }
        function byPriority(a: IDirectiveInternal, b: IDirectiveInternal): number {
            var diff = b.priority - a.priority;
            if (diff !== 0) {
                return diff;
            }
            if (a.name !== b.name) {
                return (a.name < b.name ? -1 : 1);
            }
            return a.index - b.index;
        }

        function addDirective(directives: IDirective[], name: string, mode: string, attrStartName?: string, attrEndName?: string) {
            if (hasDirectives.hasOwnProperty(name)) {
                var foundDirectives = $injector.get<IDirective[]>(name + "Directive");
                var applicableDirectives = _.filter(foundDirectives, function (dir) {
                    return dir.restrict.indexOf(mode) !== -1;
                });
                _.forEach(applicableDirectives, function (directive) {
                    if (attrStartName) {
                        directive = _.create(directive, {
                            $$start: attrStartName,
                            $$end: attrEndName
                        });
                    }
                    directives.push(directive);
                });
            }
        }

        function applyDirectivesToNode(directives: IDirectiveInternal[], compileNode: HTMLElement, attrs: IAttributes) {
            var $compileNode = $(compileNode);
            var terminalPriority = -Number.MAX_VALUE;
            var terminal = false;
            _.forEach(directives, function (directive) {
                if (directive.$$start) {
                    $compileNode = groupScan(compileNode, directive.$$start, directive.$$end);
                }
                if (directive.priority < terminalPriority) {
                    return false;
                }
                if (directive.compile) {
                    directive.compile($compileNode, attrs, undefined);
                }
                if (directive.terminal) {
                    terminal = true;
                    terminalPriority = directive.priority;
                }
            });
            return terminal;
        }

        function groupScan(node: HTMLElement, startAttr: string, endAttr: string) {
            var nodes = [];
            if (startAttr && node && node.hasAttribute(startAttr)) {
                var depth = 0;
                do {
                    if (node.nodeType === Node.ELEMENT_NODE) {
                        if (node.hasAttribute(startAttr)) {
                            depth++;
                        } else if (node.hasAttribute(endAttr)) {
                            depth--;
                        }
                    }
                    nodes.push(node);
                    node = <HTMLElement>node.nextSibling;
                } while (depth > 0);
            } else {
                nodes.push(node);
            }
            return $(nodes);
        }

        return compile;
    }];
}

$CompileProvider.$inject = ["$provide"];