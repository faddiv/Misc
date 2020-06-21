import * as _ from 'lodash';
import { AST } from './AST';
import { ISyntaxTreeElement, ISrcExpression, ICompiledExpression, ICallContext, IProgramElement, IAssignmentExpressionElement, INGValueParameter, IFilterService } from "./angularInterfaces";

"use strict";
/**
 * ASTCompiler
 */
export class ASTCompiler {
    private ast: ISyntaxTreeElement;
    private state: ISrcExpression;
    private stage: string;
    private readonly stringEscapeRegex = /[^ a-zA-Z0-9]/g;
    private static readonly unsafeMethods = ["constructor", "__proto__"
        , "__defineGetter__", "__defineSetter__"
        , "__lookupGetter__", "__lookupSetter__"];
    private static readonly CALL = Function.prototype.call;
    private static readonly APPLY = Function.prototype.apply;
    private static readonly BIND = Function.prototype.bind;

    constructor(private astBuilder: AST, private $filter: IFilterService) {
    }

    public compile(text: string): ICompiledExpression {
        var ast = this.astBuilder.ast(text);
        var extra = "";
        this.markConstantAndWatchExpressions(ast);
        this.state = {
            fn: { body: [], vars: [], },
            nextId: 0,
            filters: {},
            assign: { body: [], vars: [] },
            inputs: []
        };
        this.stage = "inputs";
        _.forEach(this.getInputs(ast.body), (input, idx) => {
            var inputKey = "fn" + idx;
            this.state[inputKey] = { body: [], vars: [] };
            this.state.computing = inputKey;
            this.state[inputKey].body.push("return " + this.recurse(input) + ";");
            this.state.inputs.push(inputKey);
        });
        this.stage = "assign";
        var assignable = this.assignableAST(ast);
        if (assignable) {
            this.state.computing = "assign";
            this.state.assign.body.push(this.recurse(assignable));
            extra = "fn.assign = function(s,v,l){" +
                (this.state.assign.vars.length ?
                    "var " + this.state.assign.vars.join(",") + ";" : "") +
                this.state.assign.body.join("") +
                "};";
        }
        this.stage = "main";
        this.state.computing = "fn";
        this.recurse(ast);
        var bodyString = this.filterPrefix() + "var fn=function(s,l){";
        if (this.state.fn.vars.length) {
            bodyString += "var " + this.state.fn.vars.join(",") + ";";
        }
        bodyString += this.state.fn.body.join("");
        bodyString += "};" +
            this.watchFns() +
            extra +
            "return fn;";
        var fn: ICompiledExpression = new Function(
            "ensureSafeMemberName",
            "ensureSafeObject",
            "ensureSafeFunction",
            "ifDefined",
            "filter",
            bodyString)(
            this.ensureSafeMemberName,
            this.ensureSafeObject,
            this.ensureSafeFunction,
            this.ifDefined,
            this.$filter);
        fn.literal = this.isLiteral(ast);
        fn.constant = ast.constant;
        return fn;
    }

    private filterPrefix() {
        if (_.isEmpty(this.state.filters)) {
            return "";
        } else {
            var parts = _.map(this.state.filters, (varName, filterName) => {
                return varName + "=" + "filter(" + this.escape(filterName) + ")";
            });
            return "var " + parts.join(",") + ";";
        }
    }

    private recurse(ast: ISyntaxTreeElement, context?: ICallContext, create?: boolean): string {
        var intoId: string;
        switch (ast.type) {
            case AST.Program:
                _.forEach(_.initial(ast.body), stmt => {
                    this.state[this.state.computing].body.push(this.recurse(stmt), ";");
                })
                this.state[this.state.computing].body.push("return ", this.recurse(_.last(ast.body)), ";");
                break;

            case AST.Literal:
                return this.escape(ast.value);

            case AST.ArrayExpression:
                var elements = _.map(ast.elements, element => {
                    return this.recurse(element);
                });
                return "[" + elements.join(",") + "]";
            case AST.ObjectExpression:
                var properties = _.map(ast.properties, property => {
                    var key;
                    switch (property.key.type) {
                        case AST.Identifier:
                            key = property.key.name;
                            break;
                        case AST.Literal:
                            key = this.escape(property.key.value);
                            break;
                    }

                    var value = this.recurse(property.value);
                    return key + ":" + value;
                });
                return "{" + properties.join(",") + "}";
            case AST.Identifier:
                this.ensureSafeMemberName(ast.name);
                intoId = this.nextId();
                var lTest;
                if (this.stage === "inputs") {
                    lTest = "false";
                } else {
                    lTest = this.getHasOwnProperty("l", ast.name);
                }
                var lAssignment = this.assign(intoId, this.nonComputedMember("l", ast.name));
                var lUndefinedTest = this.not(lTest);
                var sTest = lUndefinedTest + "&&s";
                var identifierOnS = this.nonComputedMember("s", ast.name);
                var sAssignment = this.assign(intoId, identifierOnS);
                this.if_(lTest, lAssignment);
                if (create) {
                    var propertyUndefinedOns = this.not(this.getHasOwnProperty("s", ast.name))
                    this.if_(sTest + "&&" + propertyUndefinedOns, this.assign(identifierOnS, "{}"));
                }
                this.if_(sTest, sAssignment);
                if (context) {
                    context.context = lTest + "?l:s";
                    context.name = ast.name;
                    context.computed = false;
                }
                this.addEnsureSafeObject(intoId);
                return intoId;
            case AST.ThisExpression:
                return "s";
            case AST.MemberExpression:
                intoId = this.nextId();
                var left = this.recurse(ast.object, undefined, create);
                if (context) {
                    context.context = left;
                }
                var value;
                if (ast.computed) {
                    var right = this.recurse(ast.property);
                    this.addEnsureSafeMemberName(right);
                    if (context) {
                        context.name = right;
                        context.computed = true;
                    }
                    value = this.computedMember(left, right);
                    if (create) {
                        var initAssignment = this.assign(value, "{}");
                        this.if_(this.not(value), initAssignment);
                    }
                } else {
                    this.ensureSafeMemberName(ast.property.name);
                    value = this.nonComputedMember(left, ast.property.name)
                    if (context) {
                        context.name = ast.property.name;
                        context.computed = false;
                    }
                    if (create) {
                        var initAssignment = this.assign(value, "{}");
                        this.if_(this.not(value), initAssignment);
                    }
                }
                var assignment = this.assign(intoId, this.ensureSafeObjectExpression(value));
                this.if_(left, assignment);
                return intoId;
            case AST.LocalsExpression:
                return "l";
            case AST.CallExpression:
                var callContext: ICallContext;
                var callee: string;
                var args: string[];
                if (ast.filter) {
                    callee = this.filter(ast.callee.name);
                    args = _.map(ast.arguments, arg => {
                        return this.recurse(arg);
                    });
                    return callee + "(" + args + ")";
                } else {
                    callContext = {};
                    callee = this.recurse(ast.callee, callContext);
                    args = _.map(ast.arguments, arg => {
                        return this.ensureSafeObjectExpression(this.recurse(arg));
                    });
                    if (callContext.name) {
                        this.addEnsureSafeObject(callContext.context);
                        if (callContext.computed) {
                            callee = this.computedMember(callContext.context, callContext.name);
                        } else {
                            callee = this.nonComputedMember(callContext.context, callContext.name);
                        }
                    }
                    this.addEnsureSafeFunction(callee);
                    return callee + "&&" + this.ensureSafeObjectExpression(callee + "(" + args.join(",") + ")");
                }
            case AST.AssignmentExpression:
                var leftContext: ICallContext = {};
                this.recurse(ast.left, leftContext, true);
                var leftExpr: string;
                if (leftContext.computed) {
                    leftExpr = this.computedMember(leftContext.context, leftContext.name);
                } else {
                    leftExpr = this.nonComputedMember(leftContext.context, leftContext.name);
                }
                var righExpr = this.ensureSafeObjectExpression(this.recurse(ast.right));
                return this.assign(leftExpr, righExpr);
            case AST.UnaryExpression:
                return ast.operator + "(" + this.addIfDefined(this.recurse(ast.argument), 0) + ")";
            case AST.BinaryExpression:
                return "(" + this.addIfDefined(this.recurse(ast.left), 0) + ")" +
                    ast.operator + "(" + this.addIfDefined(this.recurse(ast.right), 0) + ")";
            case AST.LogicalExpression:
                intoId = this.nextId();
                this.state[this.state.computing].body.push(this.assign(intoId, this.recurse(ast.left)));
                this.if_(ast.operator === "&&" ? intoId : this.not(intoId), this.assign(intoId, this.recurse(ast.right)));
                return intoId;
            case AST.ConditionalExpression:
                intoId = this.nextId();
                var testId = this.nextId();
                this.state[this.state.computing].body.push(this.assign(testId, this.recurse(ast.test)));
                this.if_(testId,
                    this.assign(intoId, this.recurse(ast.consequent)))
                this.if_(this.not(testId),
                    this.assign(intoId, this.recurse(ast.alternate)))
                return intoId;
            case AST.NGValueParameter:
                return "v";
            default:
                break;
        }
    }
    private filter(name: string): string {
        if (!this.state.filters.hasOwnProperty(name)) {
            this.state.filters[name] = this.nextId(true);
        }
        return this.state.filters[name];
    }
    private addIfDefined(value: string, defaultValue: any) {
        return "ifDefined(" + value + "," + this.escape(defaultValue) + ")";
    }
    private ifDefined(value: any, defaultValue: any): any {
        return typeof value === "undefined" ? defaultValue : value
    }
    private addEnsureSafeMemberName(expr: string) {
        this.state[this.state.computing].body.push("ensureSafeMemberName(", expr, ");");
    }
    private addEnsureSafeObject(expr: string) {
        this.state[this.state.computing].body.push(this.ensureSafeObjectExpression(expr), ";");
    }
    private addEnsureSafeFunction(expr: string) {
        this.state[this.state.computing].body.push("ensureSafeFunction(", expr, ");");
    }
    private ensureSafeObjectExpression(expr: string): string {
        return "ensureSafeObject(" + expr + ")";
    }
    private ensureSafeFunction(obj: any): any {
        if (obj) {
            if (obj.constructor === obj) {
                throw "Referencing Function in Angular expressions is disallowed";
            } else if (obj === ASTCompiler.CALL || obj === ASTCompiler.APPLY || obj === ASTCompiler.BIND) {
                throw "Referencing call, apply, or bind in Angular expressions is disallowed";
            }
        }
    }
    private ensureSafeObject(obj: any): any {
        if (obj) {
            if (obj.window === obj) {
                throw "Referencing window in Angular expressions is disallowed";
            } else if (obj.children && (obj.nodeName || (obj.prop && obj.attr && obj.find))) {
                throw "Referencing DOM nodes in Angular expressions is disallowed";
            } else if (obj.constructor === obj) {
                throw "Referencing Function in Angular expressions is disallowed";
            } else if (obj === Object) {
                throw "Referencing Object in Angular expressions is disallowed";
            }
        }
        return obj;
    }
    private ensureSafeMemberName(name: string) {
        if (ASTCompiler.unsafeMethods.indexOf(name) >= 0) {
            throw "Attempting to access a disallowed field in Angular expressions!";
        }
    }
    private nextId(skip?: boolean) {
        var id = "v" + (this.state.nextId++);
        if (!skip) {
            this.state[this.state.computing].vars.push(id);
        }
        return id;
    }
    private getHasOwnProperty(object: string, property: string): string {
        return object + "&&(" + this.escape(property) + " in " + object + ")";
    }
    private not(expr: string): string {
        return "!(" + expr + ")";
    }
    private assign(id: string, value: string) {
        return id + "=" + value + ";";
    }
    private computedMember(left: string, right: string): string {
        return "(" + left + ")[" + right + "]";
    }
    private nonComputedMember(left: string, right: string): string {
        return "(" + left + ")." + right;
    }

    private if_(test: string, consequent: string) {
        this.state[this.state.computing].body.push("if(", test, "){", consequent, "}");
    }

    private escape(value: any): string {
        if (_.isString(value)) {
            return "\"" + value.replace(this.stringEscapeRegex, this.stringEscapeFn) + "\"";
        } else if (_.isNull(value)) {
            return "null";
        } else {
            return value;
        }
    }

    private stringEscapeFn(c: string): string {
        return "\\u" + ("0000" + c.charCodeAt(0).toString(16)).slice(-4);
    }

    private markConstantAndWatchExpressions(ast: ISyntaxTreeElement) {
        var allConstant: boolean;
        var argsToWatch: ISyntaxTreeElement[];
        switch (ast.type) {
            case AST.Program:
                allConstant = true;
                _.forEach(ast.body, expr => {
                    this.markConstantAndWatchExpressions(expr);
                    allConstant = allConstant && expr.constant;
                });
                ast.constant = allConstant;
                break;
            case AST.Identifier:
                ast.constant = false;
                ast.toWatch = [ast];
                break;
            case AST.ArrayExpression:
                allConstant = true;
                argsToWatch = [];
                _.forEach(ast.elements, expr => {
                    this.markConstantAndWatchExpressions(expr);
                    allConstant = allConstant && expr.constant;
                    if (!expr.constant) {
                        argsToWatch.push.apply(argsToWatch, expr.toWatch);
                    }
                });
                ast.constant = allConstant;
                ast.toWatch = argsToWatch;
                break;
            case AST.ObjectExpression:
                allConstant = true;
                argsToWatch = [];
                _.forEach(ast.properties, property => {
                    this.markConstantAndWatchExpressions(property.value);
                    allConstant = allConstant && property.value.constant;
                    if (!property.value.constant) {
                        argsToWatch.push.apply(argsToWatch, property.value.toWatch);
                    }
                });
                ast.constant = allConstant;
                ast.toWatch = argsToWatch;
                break;
            case AST.MemberExpression:
                this.markConstantAndWatchExpressions(ast.object);
                if (ast.computed) {
                    this.markConstantAndWatchExpressions(ast.property);
                }
                ast.constant = ast.object.constant &&
                    (!ast.computed || ast.property.constant);
                ast.toWatch = [ast];
                break;
            case AST.CallExpression:
                var stateless = !!(ast.filter && !this.$filter(ast.callee.name).$stateful);
                allConstant = stateless;
                argsToWatch = [];
                _.forEach(ast.arguments, expr => {
                    this.markConstantAndWatchExpressions(expr);
                    allConstant = allConstant && expr.constant;
                    if (!expr.constant) {
                        argsToWatch.push.apply(argsToWatch, expr.toWatch);
                    }
                });
                ast.constant = allConstant;
                ast.toWatch = stateless ? argsToWatch : [ast];
                break;
            case AST.AssignmentExpression:
                this.markConstantAndWatchExpressions(ast.left);
                this.markConstantAndWatchExpressions(ast.right);
                ast.constant = ast.left.constant && ast.right.constant;
                ast.toWatch = [ast];
                break;
            case AST.UnaryExpression:
                this.markConstantAndWatchExpressions(ast.argument);
                ast.constant = ast.argument.constant;
                ast.toWatch = ast.argument.toWatch;
                break;
            case AST.BinaryExpression:
            case AST.LogicalExpression:
                this.markConstantAndWatchExpressions(ast.left);
                this.markConstantAndWatchExpressions(ast.right);
                ast.constant = ast.left.constant && ast.right.constant;
                if (AST.BinaryExpression) {
                    ast.toWatch = ast.left.toWatch.concat(ast.right.toWatch);
                } else {
                    ast.toWatch = [ast];
                }
                break;
            case AST.ConditionalExpression:
                this.markConstantAndWatchExpressions(ast.test);
                this.markConstantAndWatchExpressions(ast.consequent);
                this.markConstantAndWatchExpressions(ast.alternate);
                ast.constant = ast.test.constant
                    && ast.consequent.constant
                    && ast.alternate.constant;
                ast.toWatch = [ast];
                break
            case AST.Literal:
                ast.constant = true;
                ast.toWatch = [];
                break;
            case AST.ThisExpression:
            case AST.LocalsExpression:
                ast.constant = false;
                ast.toWatch = [];
                break;
        }
    }

    private isLiteral(ast: IProgramElement) {
        if (ast.body.length === 0)
            return true;
        if (ast.body.length === 1) {
            return ast.body[0].type === AST.Literal
                || ast.body[0].type === AST.ArrayExpression
                || ast.body[0].type === AST.ObjectExpression;
        }
        return false;
    }

    private getInputs(ast: ISyntaxTreeElement[]) {
        if (ast.length !== 1) {
            return;
        }
        var candidate = ast[0].toWatch;
        if (candidate.length !== 1 || candidate[0] !== ast[0]) {
            return candidate;
        }
    }

    private watchFns() {
        var result = [];
        _.forEach(this.state.inputs, (inputName) => {
            result.push("var ", inputName, "=function(s) {",
                (this.state[inputName].vars.length ?
                    "var " + this.state[inputName].vars.join(",") + ";" : ""),
                this.state[inputName].body.join(""),
                "};");
        });
        if (result.length) {
            result.push("fn.inputs = [", this.state.inputs.join(","), "];");
        }
        return result.join("");
    }

    private isAssignable(ast: ISyntaxTreeElement) {
        return ast.type == AST.Identifier || ast.type === AST.MemberExpression;
    }

    private assignableAST(ast: IProgramElement): IAssignmentExpressionElement {
        if (ast.body.length == 1 && this.isAssignable(ast.body[0])) {
            return {
                type: AST.AssignmentExpression,
                left: ast.body[0],
                right: <INGValueParameter>{
                    type: AST.NGValueParameter
                }
            };
        }
    }
}