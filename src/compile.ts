import * as _ from "lodash";
import { ICompileProvider, IDirectiveFactory, auto, Injectable, IDirective, IAttributes, IScope, ITemplateLinkingFunction } from "angular";
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
                        if (directive.link && !directive.compile) {
                            directive.compile = _.constant(directive.link);
                        }
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

    this.$get = ["$injector", "$rootScope", function ($injector: auto.IInjectorService, $rootScope: IScope) {
        class Attributes implements IAttributes {
            $attr: Object
            private $$observers: {
                [name: string]: ((value?: any) => any)[]
            };
            constructor(public $$element: JQuery) {
                this.$attr = {};
            }
            $normalize(name: string): string { return undefined; }
            $addClass(classVal: string): void {
                this.$$element.addClass(classVal);
            }
            $removeClass(classVal: string): void {
                this.$$element.removeClass(classVal);
            }
            $updateClass(newClassVal: string, oldClassVal: string): void {
                var newClasses = newClassVal.split(/\s+/);
                var oldClasses = oldClassVal.split(/\s+/);
                var addedClasses = _.difference(newClasses, oldClasses);
                var removedClasses = _.difference(oldClasses, newClasses);
                if (addedClasses.length) {
                    this.$addClass(addedClasses.join(" "));
                }
                if (removedClasses.length) {
                    this.$removeClass(removedClasses.join(" "));
                }
            }
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

                if (this.$$observers) {
                    _.forEach(this.$$observers[key], function (observer) {
                        try {
                            observer(value);
                        } catch (e) {
                            console.log(e);
                        }
                    });
                }
            }
            $observe<T>(key: string, fn: (value?: T) => any): Function {
                this.$$observers = this.$$observers || Object.create(null);
                this.$$observers[key] = this.$$observers[key] || [];
                this.$$observers[key].push(fn);
                $rootScope.$evalAsync(() => {
                    fn(this[key]);
                })
                return () => {
                    var index = this.$$observers[key].indexOf(fn);
                    if (index >= 0) {
                        this.$$observers[key].splice(index, 1);
                    }
                };
            }
        }
        function Attributes2(element: Element) {
            this.$$element = element;
        }
        function compile($compileNodes: JQuery): ITemplateLinkingFunction {
            var compositeLinkFn = compileNodes($compileNodes);

            return <any>function publicLinkFn(scope: IScope) {
                $compileNodes.data("$scope", scope);
                compositeLinkFn(scope, $compileNodes);
            };
        }

        function compileNodes($compileNodes: JQuery): ITemplateLinkingFunction {
            var linkFns = [];
            _.forEach($compileNodes, function (node: HTMLElement, i: number) {
                var attrs = new Attributes($(node));
                var directives = collectDirectives(node, attrs);
                var nodeLinkFn;
                if (directives.length) {
                    nodeLinkFn = applyDirectivesToNode(directives, node, attrs);
                }
                var childLinkFn;
                if (!nodeLinkFn || !nodeLinkFn.terminal
                    && node.childNodes && node.childNodes.length) {
                    childLinkFn = compileNodes(<any>node.childNodes);
                }
                if (nodeLinkFn && nodeLinkFn.scope) {
                    attrs.$$element.addClass("ng-scope");
                }
                if (nodeLinkFn || childLinkFn) {
                    linkFns.push({
                        nodeLinkFn: nodeLinkFn,
                        childLinkFn: childLinkFn,
                        idx: i
                    });
                }
            });
            function compositeLinkFn(scope: IScope, linkNodes: ITemplateLinkingFunction) {
                var stableNodeList = [];
                _.forEach(linkFns, function (linkFn) {
                    var nodeIdx = linkFn.idx;
                    stableNodeList[nodeIdx] = linkNodes[nodeIdx];
                })
                _.forEach(linkFns, function (linkFn) {
                    var node = stableNodeList[linkFn.idx];
                    if (linkFn.nodeLinkFn) {
                        if (linkFn.nodeLinkFn.scope) {
                            scope = scope.$new();
                            $(node).data("$scope", scope);
                        }
                        linkFn.nodeLinkFn(
                            linkFn.childLinkFn,
                            scope,
                            node
                        );
                    } else {
                        linkFn.childLinkFn(
                            scope,
                            node.childNodes
                        );
                    }
                });
            };
            return compositeLinkFn;
        }

        function collectDirectives(node: HTMLElement, attrs: IAttributes): IDirectiveInternal[] {
            var match;
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
                var className = node.className;
                if (_.isString(className) && !_.isEmpty(className)) {
                    while ((match = /([\d\w\-_]+)(?:\:([^;]+))?;?/.exec(className))) {
                        var normalizedClassName = directiveNormalize(match[1]);
                        if (addDirective(directives, normalizedClassName, "C")) {
                            attrs[normalizedClassName] = match[2] ? match[2].trim() : undefined;
                        }
                        className = className.substr(match.index + match[0].length);
                    }
                }
            } else if (node.nodeType === Node.COMMENT_NODE) {
                match = /^\s*directive\:\s*([\d\w\-_]+)\s*(.*)$/.exec(node.nodeValue);
                if (match) {
                    var normalizedName = directiveNormalize(match[1]);
                    if (addDirective(directives, normalizedName, "M")) {
                        attrs[normalizedName] = match[2] ? match[2].trim() : undefined;
                    }
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

        function addDirective(directives: IDirective[], name: string, mode: string, attrStartName?: string, attrEndName?: string): IDirective {
            var match: IDirective;
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
                    match = directive;
                });
            }
            return match
        }

        function applyDirectivesToNode(directives: IDirectiveInternal[], compileNode: HTMLElement, attrs: IAttributes) {
            var $compileNode = $(compileNode);
            var terminalPriority = -Number.MAX_VALUE;
            var terminal = false;
            var preLinks = [];
            var postLinks = [];
            var newScopeDirective: IDirectiveInternal;
            var newIsolateScopeDirective: IDirectiveInternal;

            function addLinkFns(preLinkFn, postLinkFn, attrStart, attrEnd, isolateScope: boolean) {
                if (preLinkFn) {
                    if (attrStart) {
                        preLinkFn = groupElementsLinkFnWrapper(preLinkFn, attrStart, attrEnd);
                    }
                    preLinkFn.isolateScope = isolateScope;
                    preLinks.push(preLinkFn);
                }
                if (postLinkFn) {
                    if (attrStart) {
                        postLinkFn = groupElementsLinkFnWrapper(postLinkFn, attrStart, attrEnd);
                    }
                    postLinkFn.isolateScope = isolateScope;
                    postLinks.push(postLinkFn);
                }
            }
            _.forEach(directives, function (directive) {
                if (directive.$$start) {
                    $compileNode = groupScan(compileNode, directive.$$start, directive.$$end);
                }
                if (directive.priority < terminalPriority) {
                    return false;
                }
                if (directive.scope) {
                    if (_.isObject(directive.scope)) {
                        newIsolateScopeDirective = directive;
                    } else {
                        newScopeDirective = newScopeDirective || directive;
                    }
                }
                if (directive.compile) {
                    var linkFn = directive.compile($compileNode, attrs, undefined);
                    var isolateScope = (directive === newIsolateScopeDirective);
                    var attrStart = directive.$$start;
                    var attrEnd = directive.$$end;
                    if (_.isFunction(linkFn)) {
                        addLinkFns(null, linkFn, attrStart, attrEnd, isolateScope);
                    } else if (linkFn) {
                        addLinkFns(linkFn.pre, linkFn.post, attrStart, attrEnd, isolateScope);
                    }
                }
                if (directive.terminal) {
                    terminal = true;
                    terminalPriority = directive.priority;
                }
            });

            function nodeLinkFn(childLinkFn, scope: IScope, linkNode: Element) {
                var $element = $(linkNode);

                var isolateScope: IScope;
                if (newIsolateScopeDirective) {
                    isolateScope = scope.$new(true);
                }
                _.forEach(preLinks, function (linkFn) {
                    linkFn(linkFn.isolateScope ? isolateScope : scope, $element, attrs);
                });
                if (childLinkFn) {
                    childLinkFn(scope, linkNode.childNodes)
                }
                _.forEachRight(postLinks, function (linkFn) {
                    linkFn(linkFn.isolateScope ? isolateScope : scope, $element, attrs);
                });
            }
            nodeLinkFn.terminal = terminal;
            nodeLinkFn.scope = newScopeDirective && newScopeDirective.scope;

            return nodeLinkFn;
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

        function groupElementsLinkFnWrapper(linkFn, attrStart, attrEnd) {
            return function (scope, element, attrs) {
                var group = groupScan(element[0], attrStart, attrEnd);
                return linkFn(scope, group, attrs);
            }
        }

        return compile;
    }];
}

$CompileProvider.$inject = ["$provide"];