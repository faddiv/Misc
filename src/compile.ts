import * as _ from "lodash";
import { ICompileProvider, IDirectiveFactory, auto, Injectable, IDirective, IAttributes, IScope, ITemplateLinkingFunction, ITranscludeFunction, IControllerService, IController, IHttpService } from "angular";
import { IDirectiveInternal, IDirectivesContainer, ICompositeLinkFunction, ILinkFunctionInfo, INodeLinkFunction, INodeList, IIsolateBindingContainer, IParseService, ICompiledExpressionInternal, IDirectiveInternalContainer, IControllerContainer, IDirectiveBinding, ILateBoundController, IDirectiveLinkFnInternal, IPreviousCompileContext } from "./angularInterfaces";

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

const REQUIRE_PREFIX_REGEXP = /^(\^\^?)?(\?)?(\^\^?)?/;

function nodeName(element: HTMLElement | HTMLElement[]): string {
    return _.isArray(element) ? element[0].nodeName : element.nodeName;
}

function directiveNormalize(name: string) {
    return _.camelCase(name.replace(PREFIX_REGEXP, ""));
}

function isBooleanAttribute(node: Element, attrName: string) {
    return BOOLEAN_ATTRS[attrName] && BOOLEAN_ELEMENTS[node.nodeName];
}
function parseIsolateBindings(scope: any): IIsolateBindingContainer {
    var bindings: IIsolateBindingContainer = {};
    _.forEach(scope, function (definition, scopeName) {
        var match = definition.match(/\s*([@<&]|=(\*?))(\??)\s*(\w*)\s*/);
        bindings[scopeName] = {
            mode: match[1][0],
            collection: match[2] === "*",
            optional: match[3],
            attrName: match[4] || scopeName
        };
    });
    return bindings;
}

function parseDirectiveBindings(directive: IDirectiveInternal): IDirectiveBinding {
    var bindigs: IDirectiveBinding = {};
    if (_.isObject(directive.scope)) {
        if (directive.bindToController) {
            bindigs.bindToController = parseIsolateBindings(directive.scope);
        } else {
            bindigs.isolateScope = parseIsolateBindings(directive.scope);
        }
    }
    if (_.isObject(directive.bindToController)) {
        bindigs.bindToController = parseIsolateBindings(directive.bindToController)
    }
    return bindigs;
}

function getDirectiveRequire(directive: IDirectiveInternal, name: string): string | string[] | { [controller: string]: string } {
    var require = directive.require || (directive.controller && name);
    if (!_.isArray(require) && _.isObject(require)) {
        _.forEach(require, function (value, key) {
            var prefix = value.match(REQUIRE_PREFIX_REGEXP);
            var name = value.substring(prefix[0].length);
            if (!name) {
                require[key] = prefix[0] + key;
            }
        })
    }
    return require
}

export default function $CompileProvider($provide: auto.IProvideService) {

    var hasDirectives: IDirectivesContainer = {};

    this.directive = function (name: string | { [directiveName: string]: Injectable<IDirectiveFactory> }, directiveFactory?: IDirectiveFactory) {
        if (_.isString(name)) {
            if (name === "hasOwnProperty") {
                throw "hasOwnProperty is not a valid directive name";
            }
            if (!hasDirectives.hasOwnProperty(name)) {
                hasDirectives[name] = [];
                $provide.factory(name + "Directive", ["$injector", function ($injector: auto.IInjectorService) {
                    var factories = hasDirectives[name];
                    return _.map(factories, function (factory: IDirectiveFactory, i: number) {
                        var directive: IDirectiveInternal = $injector.invoke(factory);
                        directive.restrict = directive.restrict || "EA";
                        directive.priority = directive.priority || 0;
                        if (directive.link && !directive.compile) {
                            directive.compile = _.constant(directive.link);
                        }
                        directive.$$bindings = parseDirectiveBindings(directive);
                        directive.name = directive.name || name;
                        directive.require = getDirectiveRequire(directive, directive.name);
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

    this.$get = ["$injector", "$parse", "$controller", "$rootScope", "$http", function ($injector: auto.IInjectorService, $parse: IParseService, $controller: IControllerService, $rootScope: IScope, $http: IHttpService) {
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

        function compile($compileNodes: JQuery): ITranscludeFunction {
            var compositeLinkFn = compileNodes($compileNodes);

            return <any>function publicLinkFn(scope: IScope) {
                $compileNodes.data("$scope", scope);
                compositeLinkFn(scope, $compileNodes);
            };
        }

        function compileNodes($compileNodes: INodeList): ICompositeLinkFunction {
            var linkFns: ILinkFunctionInfo[] = [];
            _.forEach($compileNodes, function (node: HTMLElement, i: number) {
                var attrs = new Attributes($(node));
                var directives = collectDirectives(node, attrs);
                var nodeLinkFn: INodeLinkFunction;
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
            function compositeLinkFn(scope: IScope, linkNodes: JQuery) {
                var stableNodeList: HTMLElement[] = [];
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
            }
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
                var directives = $injector.get<IDirective>(name + "Directive");
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
            return match;
        }

        function compileTemplateUrl(directives: IDirectiveInternal[], $compileNode: JQuery, attrs: IAttributes, previousCompileContext: IPreviousCompileContext) {
            var origAsyncDirective = directives.shift();
            var derivedSyncDirective = _.extend(
                {},
                origAsyncDirective,
                { templateUrl: null }
            );
            var templateUrl = _.isFunction(origAsyncDirective.templateUrl) ? origAsyncDirective.templateUrl($compileNode, attrs) : origAsyncDirective.templateUrl;
            $compileNode.empty();
            $http.get(templateUrl)
                .success(function (template: string) {
                    directives.unshift(derivedSyncDirective);
                    $compileNode.html(template);
                    applyDirectivesToNode(directives, $compileNode, attrs, previousCompileContext);
                    compileNodes(<any>$compileNode[0].childNodes);
                });
        }

        function applyDirectivesToNode(directives: IDirectiveInternal[], compileNode: HTMLElement | JQuery, attrs: IAttributes, previousCompileContext: IPreviousCompileContext): INodeLinkFunction {
            previousCompileContext = previousCompileContext || {};
            var $compileNode = $(compileNode);
            var terminalPriority = -Number.MAX_VALUE;
            var terminal = false;
            var preLinkFns: IDirectiveLinkFnInternal[] = [];
            var postLinkFns: IDirectiveLinkFnInternal[] = [];
            var controllers: IControllerContainer = {};
            var newScopeDirective: IDirectiveInternal;
            var newIsolateScopeDirective: IDirectiveInternal;
            var templateDirective: IDirectiveInternal = previousCompileContext.templateDirective;
            var controllerDirectives: IDirectiveInternalContainer;

            function getControllers(require: string | string[] | { [controller: string]: string }, $element: JQuery): IController | IController[] | { [key: string]: IController } {
                var value: IController | IController[] | { [key: string]: IController };
                if (_.isArray(require)) {
                    return _.map(require, function (r) {
                        return getControllers(r, $element);
                    });
                } else if (_.isObject(require)) {
                    return _.mapValues(require, function (r) {
                        return getControllers(r, $element);
                    });
                } else if (_.isString(require)) {
                    var match = require.match(REQUIRE_PREFIX_REGEXP);
                    var optional = match[2];
                    require = require.substring(match[0].length);
                    if (match[1] || match[3]) {
                        if (match[3] && !match[1]) {
                            match[1] = match[3];
                        }
                        if (match[1] === "^^") {
                            $element = $element.parent();
                        }
                        while ($element.length) {
                            value = $element.data("$" + require + "Controller");
                            if (value) {
                                break;
                            } else {
                                $element = $element.parent();
                            }
                        }
                    } else {
                        if (controllers[require]) {
                            value = controllers[require].instance;
                        }
                    }
                }
                if (!value && !optional) {
                    throw "Controller " + require + " required by directive, cannot be found!";
                }
                return value || null;
            }

            function addLinkFns(preLinkFn: IDirectiveLinkFnInternal, postLinkFn: IDirectiveLinkFnInternal, attrStart, attrEnd, isolateScope: boolean, require: string | string[] | { [controller: string]: string }) {
                if (preLinkFn) {
                    if (attrStart) {
                        preLinkFn = groupElementsLinkFnWrapper(preLinkFn, attrStart, attrEnd);
                    }
                    preLinkFn.isolateScope = isolateScope;
                    preLinkFn.require = require;
                    preLinkFns.push(preLinkFn);
                }
                if (postLinkFn) {
                    if (attrStart) {
                        postLinkFn = groupElementsLinkFnWrapper(postLinkFn, attrStart, attrEnd);
                    }
                    postLinkFn.isolateScope = isolateScope;
                    postLinkFn.require = require;
                    postLinkFns.push(postLinkFn);
                }
            }

            function initializeDirectiveBindings(scope: IScope, attrs: IAttributes, destination, bindings: IIsolateBindingContainer, newScope: IScope) {
                _.forEach(bindings, function (definition, scopeName) {
                    var attrName = definition.attrName;
                    var unwatch: () => void;
                    var parentGet: ICompiledExpressionInternal;
                    switch (definition.mode) {
                        case "@":
                            attrs.$observe(attrName, function (newAttrValue) {
                                destination[scopeName] = newAttrValue;
                            });
                            if (attrs[attrName]) {
                                destination[scopeName] = attrs[attrName];
                            }
                            break;
                        case "<":
                            if (definition.optional && !attrs[attrName])
                                break;
                            parentGet = $parse(attrs[attrName]);
                            destination[scopeName] = parentGet(scope);
                            unwatch = scope.$watch(parentGet, function (newValue) {
                                destination[scopeName] = newValue;
                            });
                            newScope.$on("$destroy", unwatch);
                            break;
                        case "=":
                            if (definition.optional && !attrs[attrName])
                                break;
                            parentGet = $parse(attrs[attrName]);
                            var lastValue = destination[scopeName] = parentGet(scope);
                            var parentValueWatch = function () {
                                var parentValue = parentGet(scope);
                                if (destination[scopeName] !== parentValue) {
                                    if (parentValue !== lastValue) {
                                        destination[scopeName] = parentValue;
                                    } else {
                                        parentValue = destination[scopeName];
                                        parentGet.assign(scope, parentValue);
                                    }
                                }
                                lastValue = parentValue;
                                return parentValue;
                            };
                            if (definition.collection) {
                                unwatch = scope.$watchCollection(attrs[attrName], parentValueWatch);
                            } else {
                                unwatch = scope.$watch(parentValueWatch);
                            }
                            newScope.$on("$destroy", unwatch);
                            break;
                        case "&":
                            var parentExpr = $parse(attrs[attrName]);
                            if (parentExpr === _.noop && definition.optional) {
                                break;
                            }
                            destination[scopeName] = function (locals) {
                                return parentExpr(scope, locals);
                            }
                            break;
                    }
                });
            }

            _.forEach(directives, function (directive, i) {
                if (directive.$$start) {
                    $compileNode = groupScan(<HTMLElement>compileNode, directive.$$start, directive.$$end);
                }
                if (directive.priority < terminalPriority) {
                    return false;
                }
                if (directive.scope) {
                    if (_.isObject(directive.scope)) {
                        if (newIsolateScopeDirective || newScopeDirective) {
                            throw "Multiple directives asking for new/inherited scope";
                        }
                        newIsolateScopeDirective = directive;
                    } else {
                        if (newIsolateScopeDirective) {
                            throw "Multiple directives asking for new/inherited scope";
                        }
                        newScopeDirective = newScopeDirective || directive;
                    }
                }
                if (directive.controller) {
                    controllerDirectives = controllerDirectives || {};
                    controllerDirectives[directive.name] = directive;
                }
                if (directive.template) {
                    if (templateDirective) {
                        throw "Multiple directives asking form template";
                    }
                    templateDirective = directive;
                    $compileNode.html(_.isFunction(directive.template)
                        ? directive.template($compileNode, attrs)
                        : directive.template);
                }
                if (directive.templateUrl) {
                    if (templateDirective) {
                        throw "Multiple directives asking form template";
                    }
                    templateDirective = directive;
                    compileTemplateUrl(
                        _.drop(directives, i),
                        $compileNode,
                        attrs,
                        { templateDirective: templateDirective });
                    return false;
                } else if (directive.compile) {
                    var linkFn = directive.compile($compileNode, attrs, undefined);
                    var isolateScope = (directive === newIsolateScopeDirective);
                    var attrStart = directive.$$start;
                    var attrEnd = directive.$$end;
                    var require = directive.require;
                    if (_.isFunction(linkFn)) {
                        addLinkFns(null, linkFn, attrStart, attrEnd, isolateScope, require);
                    } else if (linkFn) {
                        addLinkFns(linkFn.pre, linkFn.post, attrStart, attrEnd, isolateScope, require);
                    }
                }
                if (directive.terminal) {
                    terminal = true;
                    terminalPriority = directive.priority;
                }
            });

            let nodeLinkFn: INodeLinkFunction = (childLinkFn, scope: IScope, linkNode: Element) => {
                var $element = $(linkNode);
                var isolateScope: IScope;

                if (newIsolateScopeDirective) {
                    isolateScope = scope.$new(true);
                    $element.addClass("ng-isolate-scope");
                    $element.data("$isolateScope", isolateScope);
                }
                if (controllerDirectives) {
                    _.forEach(controllerDirectives, function (directive) {
                        var locals = {
                            $scope: directive === newIsolateScopeDirective ? isolateScope : scope,
                            $element: $element,
                            $attrs: attrs
                        }
                        var controllerName = directive.controller;
                        if (controllerName === "@") {
                            controllerName = attrs[directive.name]
                        }
                        var controller =
                            $controller<ILateBoundController<any>>(
                                controllerName, locals, true, directive.controllerAs);
                        controllers[directive.name] = controller;
                        $element.data("$" + directive.name + "Controller", controller.instance);
                    });
                }


                if (newIsolateScopeDirective) {
                    initializeDirectiveBindings(
                        scope,
                        attrs,
                        isolateScope,
                        newIsolateScopeDirective.$$bindings.isolateScope,
                        isolateScope);
                }
                var scopeDirective = newIsolateScopeDirective || newScopeDirective;
                if (scopeDirective && controllers[scopeDirective.name]) {
                    initializeDirectiveBindings(
                        scope,
                        attrs,
                        controllers[scopeDirective.name].instance,
                        scopeDirective.$$bindings.bindToController,
                        isolateScope);
                }

                _.forEach(controllers, function (controller) {
                    controller();
                })

                _.forEach(controllerDirectives, function (controllerDirective, name) {
                    var require = controllerDirective.require;
                    if (_.isObject(require) && !_.isArray(require) && controllerDirective.bindToController) {
                        var controller = controllers[controllerDirective.name].instance;
                        var requiredControllers = getControllers(require, $element);
                        _.assign(controller, requiredControllers);
                    }
                });
                _.forEach(preLinkFns, function (linkFn) {
                    linkFn(
                        linkFn.isolateScope ? isolateScope : scope,
                        $element,
                        attrs,
                        linkFn.require && getControllers(linkFn.require, $element));
                });
                if (childLinkFn) {
                    var scopeToChild = scope;
                    if (newIsolateScopeDirective && newIsolateScopeDirective.template) {
                        scopeToChild = isolateScope;
                    }
                    childLinkFn(scopeToChild, linkNode.childNodes)
                }
                _.forEachRight(postLinkFns, function (linkFn) {
                    linkFn(
                        linkFn.isolateScope ? isolateScope : scope,
                        $element,
                        attrs,
                        linkFn.require && getControllers(linkFn.require, $element));
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
            return function (scope, element, attrs, ctrl) {
                var group = groupScan(element[0], attrStart, attrEnd);
                return linkFn(scope, group, attrs, ctrl);
            }
        }

        return compile;
    }];
}

$CompileProvider.$inject = ["$provide"];