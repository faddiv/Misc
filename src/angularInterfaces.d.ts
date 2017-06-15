// <reference path="../typings/modules/angular/index.d.ts" />
import { IScope, IModule, IDeferred, IAngularStatic, ICompiledExpression, IFilterService, IParseService, auto, Injectable, IAngularEvent, IDirective, IDirectiveFactory, ITemplateLinkingFunction, IDirectiveLinkFn } from "angular";
import { List } from "lodash";

declare global {
    interface Window {
        angular: IAngularStatic;
    }

    interface Function {
        $inject?: ReadonlyArray<string>;
    }

}

interface IScopeInternal extends IScope {
    $$postDigest?(fn: () => void): void;
    $$watchers?: IWatcher[];
}

interface IWatcher {
    watchFn: (scope: IScope) => any;
    listenerFn: (oldValue: any, newValue: any, scope: IScope) => void;
    last?: any;
    valueEq: boolean;
}

interface IAsyncQueueItem {
    expression: (scope: IScope) => any;
    scope: IScope;
}

interface ICompiledExpressionInternal extends ICompiledExpression {

    inputs?: ((scope: IScope) => any)[];
    $$watchDelegate?(scope: IScope,
        listenerFn?: (oldValue: any, newValue: any, scope: IScope) => void,
        valueEq?: boolean,
        expr?: ICompiledExpression): () => void;
}

interface IFilterService extends IFilterService {
    <T extends IFilter>(name: string): T;
}

interface IParseService extends IParseService {
    (expression: string | Function): ICompiledExpressionInternal;
}

interface IParser {
    parse(text: string): ICompiledExpression;
}

interface IToken {
    text: string;
    value?: any;
    identifier?: boolean;
}

interface ICoreSyntaxTreeElement {
    constant?: boolean;
    toWatch?: ISyntaxTreeElement[];
}

interface IProgramElement extends ICoreSyntaxTreeElement {
    type: "Program";
    body: ISyntaxTreeElement[];
}
interface ILiteralElement extends ICoreSyntaxTreeElement {
    type: "Literal";
    value?: any;
}
interface IIdentifierElement extends ICoreSyntaxTreeElement {
    type: "Identifier";
    name: string;
}
interface IArrayExpressionElement extends ICoreSyntaxTreeElement {
    type: "ArrayExpression";
    elements: ISyntaxTreeElement[];
}
interface IObjectExpressionElement extends ICoreSyntaxTreeElement {
    type: "ObjectExpression";
    properties: IPropertyElement[]
}
interface IPropertyElement extends ICoreSyntaxTreeElement {
    type: "Property";
    key: IIdentifierOrConstant;
    value?: ISyntaxTreeElement;
}
interface IThisExpressionElement extends ICoreSyntaxTreeElement {
    type: "ThisExpression";
}
interface ILocalsExpressionElement extends ICoreSyntaxTreeElement {
    type: "LocalsExpression";
}
interface IMemberExpressionElement extends ICoreSyntaxTreeElement {
    type: "MemberExpression";
    object: ISyntaxTreeElement;
    property: IIdentifierElement;
    computed: boolean;
}
interface ICallExpressionElement extends ICoreSyntaxTreeElement {
    type: "CallExpression";
    callee: IIdentifierElement;
    arguments: ISyntaxTreeElement[];
    filter?: boolean;
}
interface IAssignmentExpressionElement extends ICoreSyntaxTreeElement {
    type: "AssignmentExpression";
    left: ISyntaxTreeElement;
    right: ISyntaxTreeElement;
}
interface IUnaryExpressionElement extends ICoreSyntaxTreeElement {
    type: "UnaryExpression";
    operator: string;
    argument: ISyntaxTreeElement;
}
interface IBinaryExpressionElement extends ICoreSyntaxTreeElement {
    type: "BinaryExpression";
    operator: string;
    left: ISyntaxTreeElement;
    right: ISyntaxTreeElement;
}
interface ILogicalExpressionElement extends ICoreSyntaxTreeElement {
    type: "LogicalExpression";
    operator: string;
    left: ISyntaxTreeElement;
    right: ISyntaxTreeElement;
}
interface IConditionalExpressionElement extends ICoreSyntaxTreeElement {
    type: "ConditionalExpression";
    test: ISyntaxTreeElement;
    consequent: ISyntaxTreeElement;
    alternate: ISyntaxTreeElement;
}
interface INGValueParameter extends ICoreSyntaxTreeElement {
    type: "NGValueParameter";
}
type IIdentifierOrConstant =
    ILiteralElement |
    IIdentifierElement;

type ISyntaxTreeElement =
    IProgramElement |
    IIdentifierOrConstant |
    IArrayExpressionElement |
    IObjectExpressionElement |
    IPropertyElement |
    IThisExpressionElement |
    IMemberExpressionElement |
    ILocalsExpressionElement |
    ICallExpressionElement |
    IAssignmentExpressionElement |
    IUnaryExpressionElement |
    IBinaryExpressionElement |
    ILogicalExpressionElement |
    IConditionalExpressionElement |
    INGValueParameter;

interface ISrcExpression {
    fn: {
        body: string[];
        vars: string[];
    };
    assign: {
        body: string[];
        vars: string[];
    };
    nextId: number;
    filters: _.Dictionary<string>;
    computing?: string;
    inputs: any[];
}
interface ICallContext {
    context?: string;
    name?: string;
    computed?: boolean;
}
interface IComparator {
    (actual: string, expected: string): boolean;
}

interface IServiceProviderFactory {
    (...args: any[]): IServiceProvider;
}

// All service providers extend this interface
interface IServiceProvider {
    $get: any;
}

interface IRootScopeProvider extends IServiceProvider {
    digestTtl(ttl: number);
}

interface IModuleInternal extends IModule {
    _invokeQueue: any[];
    _configBlocks: any[];
    _runBlocks: Injectable<Function>;
}
interface IModuleContainer {
    [name: string]: IModuleInternal;
}
interface IListenerContainer {
    [eventName: string]: ((event: IAngularEvent, ...args: any[]) => any)[];
}

interface IInjectorCache extends Object {
    [name: string]: any;
    $injector?: auto.IInjectorService;
}

interface IProviderCache extends IInjectorCache {
    $provide?: auto.IProvideService
}

interface IFilter {
    (...input): any;
    $stateful?: true;
}

interface IFilterFactory {
    (): IFilter;
}

interface IDirectiveInternal extends IDirective {
    index: number;
    name: string;
    $$start: string;
    $$end: string;
    $$bindings: IDirectiveBinding;
}

interface IDirectiveBinding {
    bindToController?: IIsolateBindingContainer;
    isolateScope?: IIsolateBindingContainer;
}

interface IPromiseState {
    value?: any;
    status?: number;
    pending?: [IDeferred<any>, (value: any) => any, (value: any) => any, (value: any) => any][];
}
type BindingType = "@" | "<" | "=" | "&";
interface IIsolateBinding {
    mode: BindingType;
    collection: boolean;
    optional: boolean;
    attrName: string;
}

type IDirectiveInternalContainer = ISimpleContainer<IDirectiveInternal> & IHasOwnProperty;

type IControllerContainer = ISimpleContainer<ILateBoundController<any>> & IHasOwnProperty;

type IIsolateBindingContainer = ISimpleContainer<IIsolateBinding> & IHasOwnProperty;

type IDirectivesContainer = ISimpleContainer<IDirectiveFactory[]> & IHasOwnProperty;
interface ISimpleContainer<T> {
    [name: string]: T;
}
interface IHasOwnProperty {
    hasOwnProperty(name: string): boolean;
}
interface ICompositeLinkFunction {
    (scope: IScope, linkNodes: JQuery);
}

interface ILinkFunctionInfo {
    nodeLinkFn: INodeLinkFunction;
    childLinkFn: IChildLinkFunction;
    idx: number;
}

interface INodeLinkFunction {
    (Elements: IChildLinkFunction, scope: IScope, node: any): void;
    scope?: any;
    terminal?: boolean;
}

interface IChildLinkFunction {
    (scope: IScope, node: any): void;
    scope: IScope;
}
interface INodeList extends List<HTMLElement> {
}

interface ILateBoundController<T> {
    (): T;
    instance: T;
}

interface IDirectiveLinkFnInternal extends IDirectiveLinkFn {
    isolateScope?: boolean;
    require?: string | string[] | { [controller: string]: string };
}
//894
//Template URL Functions
