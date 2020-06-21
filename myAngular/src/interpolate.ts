"use strict";
import { IParseService, ICompiledExpression, IInterpolationFunction, IScope, IInterpolateService } from "angular";
import * as _ from "lodash";
import { IInterpolationFunctionInternal } from "./angularInterfaces";


function stringify(value: string | Object) {
    if (_.isNull(value) || _.isUndefined(value)) {
        return "";
    } else if (_.isObject(value)) {
        return JSON.stringify(value);
    } else {
        return "" + value;
    }
}

export default function $InterpolateProvider() {
    var startSymbol = "{{";
    var endSymbol = "}}";
    function escapeChar(char: string) {
        return "\\\\\\" + char;
    }
    this.startSymbol = function (value: string) {
        if (value) {
            startSymbol = value;
            return this;
        } else {
            return startSymbol;
        }
    };
    this.endSymbol = function (value: string) {
        if (value) {
            endSymbol = value;
            return this;
        } else {
            return endSymbol;
        }
    };
    this.$get = ["$parse", function ($parse: IParseService) {
        var escapedStartMatcher =
            new RegExp(startSymbol.replace(/./g, escapeChar), "g");
        var escapedEndMatcher =
            new RegExp(endSymbol.replace(/./g, escapeChar), "g");

        function unescapeText(text: string) {
            return text.replace(escapedStartMatcher, startSymbol)
                .replace(escapedEndMatcher, endSymbol);
        }

        let $interpolate: IInterpolateService = function (text: string, mustHaveExpressions?: boolean): IInterpolationFunction {
            var index = 0;
            var parts: (string | ICompiledExpression)[] = [];
            var expressions: string[] = [];
            var expressionFns: ICompiledExpression[] = [];
            var expressionPositions: number[] = [];
            var startIndex: number;
            var endIndex: number;
            var exp: string;
            var expFn: ICompiledExpression;
            while (index < text.length) {
                startIndex = text.indexOf(startSymbol, index);
                if (startIndex !== -1) {
                    endIndex = text.indexOf(endSymbol, startIndex + startSymbol.length);
                }
                if (startIndex !== -1 && endIndex !== -1) {
                    if (startIndex !== index) {
                        parts.push(unescapeText(text.substring(index, startIndex)));
                    }
                    exp = text.substring(startIndex + startSymbol.length, endIndex);
                    expFn = $parse(exp);
                    expressions.push(exp);
                    expressionFns.push(expFn);
                    expressionPositions.push(parts.length);
                    parts.push(expFn);
                    index = endIndex + endSymbol.length;
                } else {
                    parts.push(unescapeText(text.substring(index)));
                    break;
                }
            }
            function compute(values: any[]) {
                _.forEach(values, function (value, i) {
                    parts[expressionPositions[i]] = stringify(value);
                })
                return parts.join("");
            }
            if (expressions.length || !mustHaveExpressions) {
                return _.extend(function interpolationFn(context: any) {
                    var values = _.map(expressionFns, function (expressionFn) {
                        return expressionFn(context);
                    });
                    return compute(values);
                }, <IInterpolationFunctionInternal>{
                    expressions: expressions,
                    $$watchDelegate(scope: IScope, listener) {
                        var lastValue;
                        return scope.$watchGroup(expressionFns, function (newValues: any[], oldValues: any[]) {
                            var newValue = compute(newValues);
                            listener(newValue,
                                (newValues === oldValues ? newValue : lastValue),
                                scope);
                            lastValue = newValue;
                        });
                    }
                });
            }
        }
        $interpolate.startSymbol = _.constant(startSymbol);
        $interpolate.endSymbol = _.constant(endSymbol);
        return $interpolate;
    }];
}