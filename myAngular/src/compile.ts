import * as _ from "lodash";
import { identifierForController } from "./controller";
import { ICompileProvider, IDirectiveFactory, auto, Injectable, IDirective, IAttributes, IScope, ITemplateLinkingFunction, ITranscludeFunction, IControllerService, IController, IHttpService, ICloneAttachFunction, ITemplateLinkingFunctionOptions, IInterpolateService, IComponentOptions } from "angular";
import { IDirectiveInternal, IDirectivesContainer, ILinkFunctionInfo, INodeLinkFunction, INodeList, IIsolateBindingContainer, IParseService, ICompiledExpressionInternal, IDirectiveInternalContainer, IControllerContainer, IDirectiveBinding, ILateBoundController, IDirectiveLinkFnInternal, IPreviousCompileContext, IChildLinkFunction, ITranscludeFunctionInternal, ITemplateLinkingFunctionOptionsInternal, IAttributeObserver, IChangesCollection, IChangesProperty, IScopeInternal } from "./angularInterfaces";

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

const _UNINITIALIZED_VALUE = new function UNINITIALIZED_VALUE() { };

function isCloneAttach(transcludedScope: any): transcludedScope is ICloneAttachFunction {
    return !(transcludedScope && transcludedScope.$watch && transcludedScope.$evalAsync);
}

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

function makeInjectable(template: string | Injectable<(...args: any[]) => string>, $injector: auto.IInjectorService) {
    if (_.isFunction(template) || _.isArray(template)) {
        return function ($element, $attrs) {
            return $injector.invoke(<Function>template, this, {
                $element: $element,
                $attrs: $attrs
            });
        }
    } else {
        return template;
    }
}

class SimpleChange implements IChangesProperty {
    isFirstChange(): boolean {
        return this.previousValue === _UNINITIALIZED_VALUE;
    }
    constructor(public previousValue: any, public currentValue: any) {
    }
}

export default function $CompileProvider($provide: auto.IProvideService) {

    var hasDirectives: IDirectivesContainer = {};
    var TTL = 10;

    this.onChangesTtl = function (value) {
        if (arguments.length) {
            TTL = value;
            return this;
        }
        return TTL;
    }

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

    this.component = function (name: string, options: IComponentOptions): string {

        function factory($injector: auto.IInjectorService) {
            return <IDirective>{
                restrict: "E",
                controller: options.controller,
                controllerAs: options.controllerAs ||
                identifierForController(options.controller) ||
                "$ctrl",
                scope: {},
                bindToController: options.bindings || {},
                template: makeInjectable(options.template, $injector),
                templateUrl: makeInjectable(options.templateUrl, $injector),
                transclude: options.transclude,
                require: options.require
            };
        }
        factory.$inject = ["$injector"];

        return this.directive(name, factory);
    }

    this.$get = ["$injector", "$parse", "$controller", "$rootScope", "$http", "$interpolate", function ($injector: auto.IInjectorService, $parse: IParseService, $controller: IControllerService, $rootScope: IScopeInternal, $http: IHttpService, $interpolate: IInterpolateService) {
        class Attributes implements IAttributes {
            $attr: Object
            public $$observers: {
                [name: string]: IAttributeObserver
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
                    if (!this.$$observers[key].$$inter) {
                        fn(this[key]);
                    }
                })
                return () => {
                    var index = this.$$observers[key].indexOf(fn);
                    if (index >= 0) {
                        this.$$observers[key].splice(index, 1);
                    }
                };
            }
        }
        var startSymbol = $interpolate.startSymbol();
        var endSymbol = $interpolate.endSymbol();
        var denormalizeTemplate = (startSymbol === "{{" && endSymbol === "}}") ?
            _.identity :
            function (template: string) {
                return template.replace(/\{\{/g, startSymbol)
                    .replace(/\}\}/g, endSymbol);
            };

        var onChangesQueue: Function[];
        var onChangesTtl = TTL;
        function flushOnChanges() {
            try {
                onChangesTtl--;
                if (!onChangesTtl) {
                    onChangesQueue = null;
                    throw TTL + "$onChanges() iteration reached. Aborting!";
                }
                $rootScope.$apply(function () {
                    _.forEach(onChangesQueue, function (onChangesHook) {
                        onChangesHook();
                    });
                    onChangesQueue = null;
                });
            } finally {
                onChangesTtl++;
            }
        }

        function compile($compileNodes: JQuery, maxPriority?: number): ITranscludeFunction {
            var compositeLinkFn = compileNodes($compileNodes, maxPriority);

            return <any>function publicLinkFn(scope: IScope, cloneAttachFn?: ICloneAttachFunction, options?: ITemplateLinkingFunctionOptionsInternal) {
                options = options || {};
                var parentBoundTranscludeFn = options.parentBoundTranscludeFn;
                var transcludeControllers = options.transcludeControllers;
                if (parentBoundTranscludeFn && parentBoundTranscludeFn.$$boundTransclude) {
                    parentBoundTranscludeFn = parentBoundTranscludeFn.$$boundTransclude;
                }
                $compileNodes.data("$scope", scope);
                var $linkNodes: JQuery;
                if (cloneAttachFn) {
                    $linkNodes = $compileNodes.clone();
                    cloneAttachFn($linkNodes, scope);
                } else {
                    $linkNodes = $compileNodes;
                }
                _.forEach(transcludeControllers, function (controller, name) {
                    $linkNodes.data("$" + name + "Controller", controller.instance);
                });
                $linkNodes.data("$scope", scope);
                compositeLinkFn(scope, $linkNodes, parentBoundTranscludeFn);
                return $linkNodes;
            };
        }

        function compileNodes($compileNodes: INodeList, maxPriority?: number): IChildLinkFunction {
            var linkFns: ILinkFunctionInfo[] = [];
            _.times($compileNodes.length, function (i: number) {
                var attrs = new Attributes($($compileNodes[i]));
                var directives = collectDirectives($compileNodes[i], attrs, maxPriority);
                var nodeLinkFn: INodeLinkFunction;
                if (directives.length) {
                    nodeLinkFn = applyDirectivesToNode(directives, $compileNodes[i], attrs);
                }
                var childLinkFn;
                if (!nodeLinkFn || !nodeLinkFn.terminal
                    && $compileNodes[i].childNodes && $compileNodes[i].childNodes.length) {
                    childLinkFn = compileNodes(<any>$compileNodes[i].childNodes);
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
            function compositeLinkFn(scope: IScope, linkNodes: JQuery, parentBoundTranscludeFn: ITranscludeFunctionInternal) {
                var stableNodeList: HTMLElement[] = [];
                _.forEach(linkFns, function (linkFn) {
                    var nodeIdx = linkFn.idx;
                    stableNodeList[nodeIdx] = linkNodes[nodeIdx];
                });
                _.forEach(linkFns, function (linkFn) {
                    var node = stableNodeList[linkFn.idx];
                    if (linkFn.nodeLinkFn) {
                        var childScope;
                        if (linkFn.nodeLinkFn.scope) {
                            childScope = scope.$new();
                            $(node).data("$scope", childScope);
                        } else {
                            childScope = scope;
                        }

                        var boundTranscludeFn: ITranscludeFunctionInternal;
                        if (linkFn.nodeLinkFn.transcludeOnThisElement) {
                            boundTranscludeFn = function (
                                transcludedScope: IScope, cloneAttachFn: ICloneAttachFunction, transcludeControllers, containingScope: IScope) {
                                if (!transcludedScope) {
                                    transcludedScope = scope.$new(false, containingScope);
                                }
                                return linkFn.nodeLinkFn.transclude(transcludedScope, cloneAttachFn, {
                                    transcludeControllers: transcludeControllers
                                });
                            }
                        } else if (parentBoundTranscludeFn) {
                            boundTranscludeFn = parentBoundTranscludeFn;
                        }
                        linkFn.nodeLinkFn(
                            linkFn.childLinkFn,
                            childScope,
                            node,
                            boundTranscludeFn
                        );
                    } else {
                        linkFn.childLinkFn(
                            scope,
                            node.childNodes,
                            parentBoundTranscludeFn
                        );
                    }
                });
            }
            return compositeLinkFn;
        }

        function addTextInterpolateDirective(directives: IDirectiveInternal[], text: string) {
            var interpolateFn = $interpolate(text, true);
            if (interpolateFn) {
                directives.push({
                    priority: 0,
                    compile() {
                        return function link(scope: IScope, element: JQuery) {
                            var bindings: string[] = element.parent().data("$binding") || [];
                            bindings = bindings.concat(interpolateFn.expressions);
                            element.parent().data("$binding", bindings);
                            element.parent().addClass("ng-binding");
                            scope.$watch(interpolateFn, function (newValue) {
                                element[0].nodeValue = newValue;
                            });
                        }
                    }
                });
            }
        }

        function addAttrInterpolateDirective(directives: IDirectiveInternal[], value: string, name: string) {
            var interpolateFn = $interpolate(value, true);
            if (interpolateFn) {
                directives.push({
                    priority: 100,
                    compile() {
                        return {
                            pre: function link(scope: IScope, element: JQuery, attrs: Attributes) {
                                if (/^(on[a-z]+|formaction)$/.test(name)) {
                                    throw "Interpolations for HTML DOM event attributes not allowed";
                                }
                                var newValue = attrs[name];
                                if (newValue !== value) {
                                    interpolateFn = newValue && $interpolate(newValue, true);
                                }
                                if (!interpolateFn) {
                                    return;
                                }
                                attrs.$$observers = attrs.$$observers || {};
                                attrs.$$observers[name] = attrs.$$observers[name] || [];
                                attrs.$$observers[name].$$inter = true;
                                attrs[name] = interpolateFn(scope);
                                scope.$watch(interpolateFn, function (newValue) {
                                    attrs.$set(name, newValue);
                                });
                            }
                        }
                    }
                })
            }
        }

        function collectDirectives(node: HTMLElement, attrs: IAttributes, maxPriority?: number): IDirectiveInternal[] {
            var match;
            var directives: IDirectiveInternal[] = [];
            if (node.nodeType === Node.ELEMENT_NODE) {
                var normalizedNodeName = directiveNormalize(nodeName(node).toLocaleLowerCase());
                addDirective(directives, normalizedNodeName, "E", maxPriority);
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
                    addAttrInterpolateDirective(directives, attr.value, normalizedAttrName);
                    addDirective(directives, normalizedAttrName, "A", maxPriority, attrStartName, attrEndName);
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
                        if (addDirective(directives, normalizedClassName, "C", maxPriority)) {
                            attrs[normalizedClassName] = match[2] ? match[2].trim() : undefined;
                        }
                        className = className.substr(match.index + match[0].length);
                    }
                }
            } else if (node.nodeType === Node.COMMENT_NODE) {
                match = /^\s*directive\:\s*([\d\w\-_]+)\s*(.*)$/.exec(node.nodeValue);
                if (match) {
                    var normalizedName = directiveNormalize(match[1]);
                    if (addDirective(directives, normalizedName, "M", maxPriority)) {
                        attrs[normalizedName] = match[2] ? match[2].trim() : undefined;
                    }
                }
            } else if (node.nodeType === Node.TEXT_NODE) {
                addTextInterpolateDirective(directives, node.nodeValue);
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

        function addDirective(directives: IDirective[], name: string, mode: string, maxPriority?: number, attrStartName?: string, attrEndName?: string): IDirective {
            var match: IDirective;
            if (hasDirectives.hasOwnProperty(name)) {
                var foundDirectives = $injector.get<IDirective[]>(name + "Directive");
                var applicableDirectives = _.filter(foundDirectives, function (dir) {
                    return (maxPriority === undefined || maxPriority > dir.priority) &&
                        dir.restrict.indexOf(mode) !== -1;
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

        function compileTemplateUrl(directives: IDirectiveInternal[], $compileNode: JQuery, attrs: IAttributes, previousCompileContext: IPreviousCompileContext): INodeLinkFunction {
            var origAsyncDirective = directives.shift();
            var derivedSyncDirective = _.extend(
                {},
                origAsyncDirective,
                {
                    templateUrl: null,
                    transclude: null
                }
            );
            var templateUrl = _.isFunction(origAsyncDirective.templateUrl) ? origAsyncDirective.templateUrl($compileNode, attrs) : origAsyncDirective.templateUrl;
            var afterTemplateNodeLinkFn: INodeLinkFunction;
            var afterTemplateChildLinkFn: IChildLinkFunction;
            var linkQueue: { scope: IScope, linkNode: any, boundTranscludeFn: ITranscludeFunctionInternal }[] = [];
            $compileNode.empty();
            $http.get(templateUrl).success(function (template: string) {
                template = denormalizeTemplate(template);
                directives.unshift(derivedSyncDirective);
                $compileNode.html(template);
                afterTemplateNodeLinkFn = applyDirectivesToNode(
                    directives, $compileNode, attrs, previousCompileContext);
                afterTemplateChildLinkFn = compileNodes(<any>$compileNode[0].childNodes);
                _.forEach(linkQueue, function (linkCall) {
                    afterTemplateNodeLinkFn(
                        afterTemplateChildLinkFn,
                        linkCall.scope,
                        linkCall.linkNode,
                        linkCall.boundTranscludeFn);
                });
                linkQueue = null;
            });
            return function delayedNodeLinkFn(elements: IChildLinkFunction, scope: IScope, linkNode: JQuery, boundTranscludeFn: ITranscludeFunctionInternal): void {
                if (linkQueue) {
                    linkQueue.push({ scope: scope, linkNode: linkNode, boundTranscludeFn: boundTranscludeFn });
                } else {
                    afterTemplateNodeLinkFn(
                        afterTemplateChildLinkFn, scope, linkNode, boundTranscludeFn);
                }
            };
        }

        function applyDirectivesToNode(directives: IDirectiveInternal[], compileNode: HTMLElement | JQuery, attrs: IAttributes, previousCompileContext?: IPreviousCompileContext): INodeLinkFunction {
            previousCompileContext = previousCompileContext || {};
            var $compileNode = $(compileNode);
            var terminalPriority = -Number.MAX_VALUE;
            var terminal = false;
            var preLinkFns: IDirectiveLinkFnInternal[] = previousCompileContext.preLinkFns || [];
            var postLinkFns: IDirectiveLinkFnInternal[] = previousCompileContext.postLinkFns || [];
            var controllers: IControllerContainer = {};
            var newScopeDirective: IDirectiveInternal;
            var newIsolateScopeDirective: IDirectiveInternal = previousCompileContext.newIsolateScopeDirective;
            var templateDirective: IDirectiveInternal = previousCompileContext.templateDirective;
            var controllerDirectives: IDirectiveInternalContainer = previousCompileContext.controllerDirectives;
            var childTranscludeFn: ITranscludeFunction;
            var hasTranscludeDirective: boolean = previousCompileContext.hasTranscludeDirective;
            var hasElementTranscludeDirective: boolean;

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

            function initializeDirectiveBindings(scope: IScope, attrs: IAttributes, destination, bindings: IIsolateBindingContainer, newScope: IScope): IChangesCollection {
                var initialChanges: IChangesCollection = {}
                var changes: IChangesCollection;
                function recordChanges(key: string, currentValue: any, previousValue: any) {
                    if (destination.$onChanges && currentValue !== previousValue) {
                        if (!onChangesQueue) {
                            onChangesQueue = [];
                            $rootScope.$$postDigest(flushOnChanges);
                        }
                        if (!changes) {
                            changes = {};
                            onChangesQueue.push(triggerOnChanges);
                        }
                        if (changes[key]) {
                            previousValue = changes[key].previousValue;
                        }
                        changes[key] = new SimpleChange(previousValue, currentValue);
                    }
                }

                function triggerOnChanges() {
                    try {
                        destination.$onChanges(changes);
                    } finally {
                        changes = null;
                    }
                }

                _.forEach(bindings, function (definition, scopeName) {
                    var attrName = definition.attrName;
                    var unwatch: () => void;
                    var parentGet: ICompiledExpressionInternal;
                    switch (definition.mode) {
                        case "@":
                            attrs.$observe(attrName, function (newAttrValue) {
                                var oldValue = destination[scopeName];
                                destination[scopeName] = newAttrValue;
                                recordChanges(scopeName, destination[scopeName], oldValue);
                            });
                            if (attrs[attrName]) {
                                destination[scopeName] = $interpolate(attrs[attrName])(scope);
                            }
                            initialChanges[scopeName] = new SimpleChange(_UNINITIALIZED_VALUE, destination[scopeName]);
                            break;
                        case "<":
                            if (definition.optional && !attrs[attrName])
                                break;
                            parentGet = $parse(attrs[attrName]);
                            destination[scopeName] = parentGet(scope);
                            unwatch = scope.$watch(parentGet, function (newValue) {
                                var oldValue = destination[scopeName];
                                destination[scopeName] = newValue;
                                recordChanges(scopeName, destination[scopeName], oldValue);
                            });
                            newScope.$on("$destroy", unwatch);
                            initialChanges[scopeName] = new SimpleChange(_UNINITIALIZED_VALUE, destination[scopeName]);
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
                return initialChanges;
            }

            let nodeLinkFn: INodeLinkFunction = (childLinkFn, scope: IScope, linkNode: Element, boundTranscludeFn: ITranscludeFunctionInternal) => {
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
                            $transclude: scopeBoundTranscludeFn,
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
                    controllers[scopeDirective.name].initialChanges = initializeDirectiveBindings(
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

                _.forEach(controllers, function (controller) {
                    var controllerInstance = controller.instance;
                    if (controllerInstance.$onInit) {
                        controllerInstance.$onInit();
                    }
                    if (controllerInstance.$onChanges) {
                        controllerInstance.$onChanges(controller.initialChanges);
                    }
                    if (controllerInstance.$onDestroy) {
                        (newIsolateScopeDirective ? isolateScope : scope).$on("$destroy", function () {
                            controllerInstance.$onDestroy();
                        });
                    }
                });

                function scopeBoundTranscludeFn(transcludedScope: IScope | ICloneAttachFunction, cloneAttachFn?: ICloneAttachFunction) {
                    var transcludeControllers;
                    if (isCloneAttach(transcludedScope)) {
                        cloneAttachFn = transcludedScope;
                        transcludedScope = <IScope>undefined;
                    }
                    if (hasElementTranscludeDirective) {
                        transcludeControllers = controllers;
                    }
                    return boundTranscludeFn(transcludedScope, cloneAttachFn, transcludeControllers, scope);
                }
                scopeBoundTranscludeFn.$$boundTransclude = boundTranscludeFn;
                _.forEach(preLinkFns, function (linkFn) {
                    linkFn(
                        linkFn.isolateScope ? isolateScope : scope,
                        $element,
                        attrs,
                        linkFn.require && getControllers(linkFn.require, $element),
                        scopeBoundTranscludeFn);
                });
                if (childLinkFn) {
                    var scopeToChild = scope;
                    if (newIsolateScopeDirective &&
                        (newIsolateScopeDirective.template || newIsolateScopeDirective.templateUrl === null)) {
                        scopeToChild = isolateScope;
                    }
                    childLinkFn(scopeToChild, linkNode.childNodes, boundTranscludeFn);
                }

                _.forEachRight(postLinkFns, function (linkFn) {
                    linkFn(
                        linkFn.isolateScope ? isolateScope : scope,
                        $element,
                        attrs,
                        linkFn.require && getControllers(linkFn.require, $element),
                        scopeBoundTranscludeFn);
                });

                _.forEach(controllers, function (controller) {
                    var controllerInstance = controller.instance;
                    if (controllerInstance.$postLink) {
                        controllerInstance.$postLink();
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
                if (directive.scope && !directive.templateUrl) {
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
                if (directive.transclude) {
                    if (hasTranscludeDirective) {
                        throw "Multiple directive asking for transclude";
                    }
                    hasTranscludeDirective = true;
                    if (directive.transclude === "element") {
                        hasElementTranscludeDirective = true;
                        var $originalCompileNode = $compileNode;
                        $compileNode = attrs.$$element = $(document.createComment(" " + directive.name + ": " + attrs[directive.name] + " "));
                        $originalCompileNode.replaceWith($compileNode);
                        terminalPriority = directive.priority;
                        childTranscludeFn = compile($originalCompileNode, terminalPriority);
                    } else {
                        var $transcludeNodes = $compileNode.clone().contents();
                        childTranscludeFn = compile($transcludeNodes);
                        $compileNode.empty();
                    }
                }
                if (directive.template) {
                    if (templateDirective) {
                        throw "Multiple directives asking form template";
                    }
                    templateDirective = directive;
                    var template = _.isFunction(directive.template)
                        ? directive.template($compileNode, attrs)
                        : directive.template;
                    template = denormalizeTemplate(template);
                    $compileNode.html(template);
                }
                if (directive.templateUrl) {
                    if (templateDirective) {
                        throw "Multiple directives asking form template";
                    }
                    templateDirective = directive;
                    nodeLinkFn = compileTemplateUrl(
                        _.drop(directives, i),
                        $compileNode,
                        attrs,
                        {
                            templateDirective: templateDirective,
                            newIsolateScopeDirective: newIsolateScopeDirective,
                            controllerDirectives: controllerDirectives,
                            hasTranscludeDirective: hasTranscludeDirective,
                            preLinkFns: preLinkFns,
                            postLinkFns: postLinkFns
                        });
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

            nodeLinkFn.terminal = terminal;
            nodeLinkFn.scope = !!(newScopeDirective && newScopeDirective.scope);//modified to be boolean;
            nodeLinkFn.transcludeOnThisElement = hasTranscludeDirective;
            nodeLinkFn.transclude = childTranscludeFn;

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
            return function (scope, element, attrs, ctrl, transclude) {
                var group = groupScan(element[0], attrStart, attrEnd);
                return linkFn(scope, group, attrs, ctrl, transclude);
            }
        }

        return compile;
    }];
}

$CompileProvider.$inject = ["$provide"];