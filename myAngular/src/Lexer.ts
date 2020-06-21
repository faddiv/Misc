import { IToken } from "./angularInterfaces";

"use strict";

const ESCAPES = {
    'n': '\n', 'f': '\f', 'r': '\r', 't': '\t',
    'v': '\v', '\'': '\'', '"': '"'
};

const OPERATORS = {
    '+': true,
    '!': true,
    '-': true,
    '*': true,
    '/': true,
    '%': true,
    '=': true,
    '==': true,
    '!=': true,
    '===': true,
    '!==': true,
    '<': true,
    '>': true,
    '<=': true,
    '>=': true,
    '&&': true,
    '||': true,
    '|': true
};

/**
 * Lexer
 */
export class Lexer {
    private text: string;
    private index: 0;
    private ch: string;
    private tokens: IToken[];
    constructor() {

    }

    lex(text: string): IToken[] {
        this.text = text;
        this.index = 0;
        this.ch = undefined;
        this.tokens = [];
        var chs: string;
        while (this.index < this.text.length) {
            this.ch = this.text.charAt(this.index);
            if (this.isNumber(this.ch) ||
                (this.is(".") && this.isNumber(this.peek()))) {
                this.readNumber();
            } else if (this.is("'\"")) {
                this.readString(this.ch);
            } else if (this.is("[],{}:.()?;")) {
                this.tokens.push({
                    text: this.ch
                });
                this.index++;
            } else if (this.isIdent(this.ch)) {
                this.readIdent();
            } else if (this.isWhitespace(this.ch)) {
                this.index++;
            } else if (chs = this.getOperator(this.ch)) {
                this.tokens.push({text: chs});
                this.index+=chs.length;
            } else {
                throw "Unexpected next character: " + this.ch;
            }
        }
        return this.tokens;
    }
    private substr(i: number) {
        return this.text.substr(this.index, i);
    }
    private getOperator(chr: string): string {
        var operator = this.substr(3);
        if(OPERATORS[operator]){
            return operator;
        }
        operator = this.substr(2);
        if(OPERATORS[operator]){
            return operator;
        }
        if(OPERATORS[this.ch]) {
            return this.ch;
        }
    }
    private is(chs: string) {
        return chs.indexOf(this.ch) >= 0;
    }
    private readIdent() {
        var text = "";
        while (this.index < this.text.length) {
            var ch = this.text.charAt(this.index);
            if(this.isIdent(ch) || this.isNumber(ch)) {
                text += ch;
            } else{
                break;
            }
        this.index++;
        }
        this.tokens.push({
            text: text,
            identifier: true
        });
    }
    private isNumber(ch: string): boolean {
        return "0" <= ch && ch <= "9";
    }

    private readNumber() {
        var number = "";
        while (this.index < this.text.length) {
            var ch = this.text.charAt(this.index).toLowerCase();
            if (ch === "." || this.isNumber(ch)) {
                number += ch;
            } else {
                var nextCh = this.peek();
                var prevCh = number.charAt(number.length - 1);
                if (ch === "e" && this.isExpOperator(nextCh)) {
                    number += ch;
                } else if (this.isExpOperator(ch) && prevCh === "e" && nextCh && this.isNumber(nextCh)) {
                    number += ch;
                } else if (this.isExpOperator(ch) && prevCh === "e" && (!nextCh || !this.isNumber(nextCh))) {
                    throw "Invalid Exponent at char " + this.index;
                } else {
                    break;
                }
            }
            this.index++;
        }
        this.tokens.push({
            text: number,
            value: Number(number)
        });
    }

    private readString(quote: string) {
        this.index++;
        var str = "";
        var escape = false;
        var rawString = quote;
        while (this.index < this.text.length) {
            var ch = this.text.charAt(this.index);
            rawString += ch;
            if (escape) {
                if (ch === "u") {
                    var hex = this.text.substring(this.index + 1, this.index + 5);
                    if (!hex.match(/[\da-f]{4}/i)) {
                        throw "Invalid unicode escape at" + this.index;
                    }
                    this.index += 4;
                    str += String.fromCharCode(parseInt(hex, 16));
                } else {
                    var replacement = ESCAPES[ch];
                    if (replacement) {
                        str += replacement;
                    } else {
                        str += ch;
                    }
                }
                escape = false;
            } else if (ch === quote) {
                this.index++;
                this.tokens.push({
                    text: rawString,
                    value: str
                });
                return;
            } else if (ch === "\\") {
                escape = true;
            } else {
                str += ch;
            }
            this.index++;
        }
        throw "Unmatched quote";
    }

    private peek(): string {
        return this.index < this.text.length ?
            this.text.charAt(this.index + 1)
            : <any>false;
    }
    private isExpOperator(ch: string): boolean {
        return ch === "-" || ch === "+" || this.isNumber(ch);
    }

    private isWhitespace(ch: string) {
        return ch === " "
            || ch === "\r"
            || ch === "\t"
            || ch === "\n"
            || ch === "\v"
            || ch === "\u00A0";
    }

    private isIdent(ch: string) {
        return (ch >= "a" && ch <= "z")
            || (ch >= "A" && ch <= "Z")
            || ch === "_" || ch === "$";
    }
}