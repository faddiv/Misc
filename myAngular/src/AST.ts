import { Lexer } from './Lexer';
import { ILiteralElement, IThisExpressionElement, ILocalsExpressionElement, IToken, IProgramElement, ISyntaxTreeElement, ICallExpressionElement, IAssignmentExpressionElement, IConditionalExpressionElement, ILogicalExpressionElement, IBinaryExpressionElement, IUnaryExpressionElement, IMemberExpressionElement, IObjectExpressionElement, IPropertyElement, IIdentifierElement, IArrayExpressionElement } from "./angularInterfaces";
"use strict";

/**
 * Lexer
 */
export class AST {
    public static readonly Program = "Program";
    public static readonly Literal = "Literal";
    public static readonly ArrayExpression = "ArrayExpression";
    public static readonly ObjectExpression = "ObjectExpression";
    public static readonly Property = "Property";
    public static readonly Identifier = "Identifier";
    public static readonly ThisExpression = "ThisExpression";
    public static readonly MemberExpression = "MemberExpression";
    public static readonly LocalsExpression = "LocalsExpression";
    public static readonly CallExpression = "CallExpression";
    public static readonly AssignmentExpression = "AssignmentExpression";
    public static readonly UnaryExpression = "UnaryExpression";
    public static readonly BinaryExpression = "BinaryExpression";
    public static readonly LogicalExpression = "LogicalExpression";
    public static readonly ConditionalExpression = "ConditionalExpression";
    public static readonly NGValueParameter = "NGValueParameter";


    private static readonly constants = {
        "null": <ILiteralElement>{ type: AST.Literal, value: null },
        "true": <ILiteralElement>{ type: AST.Literal, value: true },
        "false": <ILiteralElement>{ type: AST.Literal, value: false },
        "this": <IThisExpressionElement>{ type: AST.ThisExpression },
        "$locals": <ILocalsExpressionElement>{ type: AST.LocalsExpression }
    };
    private tokens: IToken[];

    constructor(private lexer: Lexer) {

    }

    public ast(text: string): IProgramElement {
        this.tokens = this.lexer.lex(text);
        var sytaxTree = this.program();
        //console.log(sytaxTree);
        return sytaxTree;
    }

    private program(): IProgramElement {
        var body: ISyntaxTreeElement[] = [];
        while (true) {
            if (this.tokens.length) {
                body.push(this.filter());
            }
            if (!this.expect(";")) {
                return {
                    type: AST.Program,
                    body: body
                };
            }
        }
    }

    private filter(): ISyntaxTreeElement {
        var left = this.assignment();
        while (this.expect("|")) {
            var args = [left];
            left = <ICallExpressionElement>{
                type: AST.CallExpression,
                callee: this.identifier(),
                arguments: args,
                filter: true
            };
            while (this.expect(":")) {
                args.push(this.assignment());
            }
        }
        return left;
    }

    private assignment(): ISyntaxTreeElement {
        var left = this.ternary();
        if (this.expect("=")) {
            var right = this.ternary();
            return <IAssignmentExpressionElement>{
                type: AST.AssignmentExpression,
                left: left,
                right: right
            };
        }
        return left;
    }

    private ternary() {
        var test = this.logicalOR();
        if (this.expect("?")) {
            var consequent = this.assignment();
            if (this.consume(":")) {
                var alternate = this.assignment();
                return <IConditionalExpressionElement>{
                    type: AST.ConditionalExpression,
                    test: test,
                    consequent: consequent,
                    alternate: alternate
                };
            }
        }
        return test;
    }

    private logicalOR(): ISyntaxTreeElement {
        var left = this.logicalAND();
        var token: IToken;
        while (token = this.expect("||")) {
            left = <ILogicalExpressionElement>{
                type: AST.LogicalExpression,
                left: left,
                operator: token.text,
                right: this.logicalAND()
            };
        }
        return left;
    }

    private logicalAND(): ISyntaxTreeElement {
        var left = this.equality();
        var token: IToken;
        while (token = this.expect("&&")) {
            left = <ILogicalExpressionElement>{
                type: AST.LogicalExpression,
                left: left,
                operator: token.text,
                right: this.equality()
            };
        }
        return left;
    }

    private equality(): ISyntaxTreeElement {
        var left = this.relational();
        var token: IToken;
        while ((token = this.expect("==", "!=", "===", "!=="))) {
            left = <IBinaryExpressionElement>{
                type: AST.BinaryExpression,
                left: left,
                operator: token.text,
                right: this.relational()
            };
        }
        return left;
    }

    private relational(): ISyntaxTreeElement {
        var left = this.additive();
        var token: IToken;
        while ((token = this.expect("<", ">", "<=", ">="))) {
            left = <IBinaryExpressionElement>{
                type: AST.BinaryExpression,
                left: left,
                operator: token.text,
                right: this.additive()
            };
        }
        return left;
    }

    private additive(): ISyntaxTreeElement {
        var left = this.multiplicative();
        var token: IToken;
        while (token = this.expect("+", "-")) {
            left = <IBinaryExpressionElement>{
                type: AST.BinaryExpression,
                left: left,
                operator: token.text,
                right: this.multiplicative()
            };
        }
        return left;
    }

    private multiplicative(): ISyntaxTreeElement {
        var left = this.unary();
        var token: IToken;
        while (token = this.expect("*", "/", "%")) {
            left = <IBinaryExpressionElement>{
                type: AST.BinaryExpression,
                left: left,
                operator: token.text,
                right: this.unary()
            };
        }
        return left;
    }

    private unary(): ISyntaxTreeElement {
        var token = this.expect("+", "!", "-");
        if (token) {
            return <IUnaryExpressionElement>{
                type: AST.UnaryExpression,
                operator: token.text,
                argument: this.unary()
            };
        } else {
            return this.primary();
        }
    }

    private primary(): ISyntaxTreeElement {
        var primary: ISyntaxTreeElement;
        if (this.expect("(")) {
            primary = this.filter();
            this.consume(")");
        } else if (this.expect("[")) {
            primary = this.arrayDeclaration();
        } else if (this.expect("{")) {
            primary = this.object();
        } else if (AST.constants.hasOwnProperty(this.tokens[0].text)) {
            primary = AST.constants[this.consume().text];
        } else if (this.peek().identifier) {
            primary = this.identifier();
        } else {
            primary = this.constant();
        }
        var next: IToken;
        while (next = this.expect(".", "[", "(")) {
            if (next.text === "[") {
                primary = <IMemberExpressionElement>{
                    type: AST.MemberExpression,
                    object: primary,
                    property: this.primary(),
                    computed: true
                };
                this.consume("]");
            } else if (next.text === ".") {
                primary = <IMemberExpressionElement>{
                    type: AST.MemberExpression,
                    object: primary,
                    property: this.identifier(),
                    computed: false
                };
            } else if (next.text === "(") {
                primary = <ICallExpressionElement>{
                    type: AST.CallExpression,
                    callee: primary,
                    arguments: this.parseArguments()
                };
                this.consume(")");
            }
        }
        return primary;
    }

    private object(): IObjectExpressionElement {
        var properties: IPropertyElement[] = [];
        if (!this.peek("}")) {
            do {
                var property: IPropertyElement = {
                    type: AST.Property,
                    key: this.peek().identifier
                        ? this.identifier()
                        : this.constant()
                };
                this.consume(":");
                property.value = this.assignment();
                properties.push(property);
            } while (this.expect(","));
        }
        this.consume("}");
        return {
            type: AST.ObjectExpression,
            properties: properties
        };
    }

    private parseArguments(): ISyntaxTreeElement[] {
        var args: ISyntaxTreeElement[] = [];
        if (!this.peek(")")) {
            do {
                args.push(this.assignment());
            } while (this.expect(","));
        }
        return args;
    }
    
    private identifier(): IIdentifierElement {
        return {
            type: AST.Identifier,
            name: this.consume().text
        };
    }
    private arrayDeclaration(): IArrayExpressionElement {
        var elements: ISyntaxTreeElement[] = [];
        if (!this.peek("]")) {
            do {
                if (this.peek("]")) {
                    break;
                }
                elements.push(this.assignment());
            } while (this.expect(","));
        }
        this.consume("]");
        return {
            type: AST.ArrayExpression,
            elements: elements
        };
    }

    private constant(): ILiteralElement {
        return {
            type: AST.Literal,
            value: this.consume().value
        };
    }

    private consume(e?: string): IToken {
        var token = this.expect(e);
        if (!token) {
            throw "Unexpected token. Expecting: " + e;
        }
        return token;
    }

    private expect(e1: string, e2?: string, e3?: string, e4?: string): IToken {
        var token = this.peek(e1, e2, e3, e4);
        if (token) {
            return this.tokens.shift();
        }
    }

    private peek(e1?: string, e2?: string, e3?: string, e4?: string) {
        if (this.tokens.length > 0) {
            var text = this.tokens[0].text;
            if (text === e1 || text === e2 || text === e3 || text === e4
                || (!e1 && !e2 && !e3 && !e4)) {
                return this.tokens[0];
            }
        }
    }
}