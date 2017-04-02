import { AST } from './AST';
import { ASTCompiler } from './ASTCompiler';
import { Lexer } from './Lexer';
import * as _ from 'lodash';
import { IScopeInternal, ICompiledExpressionInternal, IFilterService, IParseService, IParser } from "./angularInterfaces";
import { IScope, IParseProvider } from "angular";
"use strict";

/**
 * Parser
 */
class Parser implements IParser {
    private ast: AST;
    private astCompiler: ASTCompiler;
    constructor(private lexer: Lexer, private $filter: IFilterService) {
        this.ast = new AST(this.lexer);
        this.astCompiler = new ASTCompiler(this.ast, $filter);
    }
    public parse(text: string): ICompiledExpressionInternal {
        return this.astCompiler.compile(text);
    }
}
/**
 * ParseProvider
 */
export default class ParseProvider implements IParseProvider {
    logPromiseWarnings(): boolean;
    logPromiseWarnings(value: boolean): IParseProvider;
    logPromiseWarnings(value?: any): boolean | IParseProvider {
        throw new Error('Method not implemented.');
    }
    unwrapPromises(): boolean;
    unwrapPromises(value: boolean): IParseProvider;
    unwrapPromises(value?: any): boolean | IParseProvider {
        throw new Error('Method not implemented.');
    }
    addLiteral(literalName: string, literalValue: any): void {
        throw new Error('Method not implemented.');
    }
    setIdentifierFns(identifierStart?: (character: string, codePoint: number) => boolean, identifierContinue?: (character: string, codePoint: number) => boolean): void {
        throw new Error('Method not implemented.');
    }


    constructor() {

    }

    $get($filter: IFilterService): IParseService {
        function parse(expression: string | ICompiledExpressionInternal): ICompiledExpressionInternal {
            if (_.isString(expression)) {
                var lexer = new Lexer();
                var parser = new Parser(lexer, $filter);
                var oneTime = false;
                if (expression.charAt(0) === ":" && expression.charAt(1) === ":") {
                    oneTime = true;
                    expression = expression.substring(2);
                }
                var parseFn = parser.parse(expression);
                if (parseFn.constant) {
                    parseFn.$$watchDelegate = constantWatchDelegate;
                } else if (oneTime) {
                    parseFn.$$watchDelegate = parseFn.literal
                        ? oneTimeLiteralWatchDelegate
                        : oneTimeWatchDelegate;
                } else if (parseFn.inputs) {
                    parseFn.$$watchDelegate = inputsWatchDelegate;
                }
                return parseFn;
            } else if (_.isFunction(expression)) {
                return expression;
            }
            return <any>function() {  };//??
        }
        return parse;
    }
}
/**
 * IParseService
 */


function constantWatchDelegate(scope: IScope,
    listenerFn: (oldValue: any, newValue: any, scope: IScope) => void,
    valueEq: boolean,
    watchFn: ICompiledExpressionInternal): () => void {
    var unwatch = scope.$watch(
        function () {
            return watchFn(scope);
        },
        function (newValue, oldValue, scope) {
            if (_.isFunction(listenerFn)) {
                listenerFn.apply(this, arguments);
            }
            unwatch();
        }, valueEq
    )
    return unwatch;
}

function oneTimeWatchDelegate(scope: IScopeInternal,
    listenerFn: (oldValue: any, newValue: any, scope: IScope) => void,
    valueEq: boolean,
    watchFn: ICompiledExpressionInternal): () => void {
    var lastValue;
    var unwatch = scope.$watch(
        function () {
            return watchFn(scope);
        },
        function (newValue, oldValue, scope: IScopeInternal) {
            lastValue = newValue;
            if (_.isFunction(listenerFn)) {
                listenerFn.apply(this, arguments);
            }
            if (!_.isUndefined(newValue)) {
                scope.$$postDigest(function () {
                    if (!_.isUndefined(lastValue)) {
                        unwatch();
                    }
                });
            }
        }, valueEq
    )
    return unwatch;
}

function oneTimeLiteralWatchDelegate(scope: IScopeInternal,
    listenerFn: (oldValue: any, newValue: any, scope: IScope) => void,
    valueEq: boolean,
    watchFn: ICompiledExpressionInternal): () => void {
    function isAllDefined(val) {
        return !_.some(val, _.isUndefined);
    }
    var unwatch = scope.$watch(
        function () {
            return watchFn(scope);
        },
        function (newValue, oldValue, scope: IScopeInternal) {
            if (_.isFunction(listenerFn)) {
                listenerFn.apply(this, arguments);
            }
            if (isAllDefined(newValue)) {
                scope.$$postDigest(function () {
                    if (isAllDefined(newValue)) {
                        unwatch();
                    }
                });
            }
        }, valueEq
    )
    return unwatch;
}

function inputsWatchDelegate(scope: IScope,
    listenerFn: (oldValue: any, newValue: any, scope: IScope) => void,
    valueEq: boolean,
    watchFn: ICompiledExpressionInternal): () => void {
    var inputExpressions = watchFn.inputs;
    var oldValues = _.times(inputExpressions.length, _.constant(() => { }));
    var lastResult;
    var unwatch = scope.$watch(function () {
        var changed = false;
        _.forEach(inputExpressions, function (inputExpr, i) {
            var newValue = inputExpr(scope);
            if (changed || !expressionInputDirtyCheck(newValue, oldValues[i])) {
                changed = true;
                oldValues[i] = newValue;
            }
        });
        if (changed) {
            lastResult = watchFn(scope);
        }
        return lastResult;
    }, listenerFn, valueEq
    );
    return unwatch;
}

function expressionInputDirtyCheck(newValue: any, oldValue: any) {
    return newValue === oldValue ||
        (typeof newValue === "number" && typeof oldValue === "number"
            && isNaN(newValue) && isNaN(oldValue));
}
