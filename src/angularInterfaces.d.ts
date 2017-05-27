// <reference path="../typings/modules/angular/index.d.ts" />
import { IScope, IModule, IDeferred, IAngularStatic, ICompiledExpression, IFilterService, IParseService, auto, Injectable, IAngularEvent, IDirective } from "angular";

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
}

interface IPromiseState {
    value?: any;
    status?: number;
    pending?: [IDeferred<any>, (value: any) => any, (value: any) => any, (value: any) => any][];
}

//762
//Plain Directive Link Functions
