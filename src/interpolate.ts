"use strict";
import { IParseService, ICompiledExpression, IInterpolationFunction, IScope } from "angular";
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

function unescapeText(text: string) {
    return text.replace(/\\{\\{/g, "{{")
        .replace(/\\}\\}/g, "}}");
}

export default function $InterpolateProvider() {
    this.$get = ["$parse", function ($parse: IParseService) {
        function $interpolate(text: string, mustHaveExpressions?: boolean): IInterpolationFunction {
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
                startIndex = text.indexOf("{{", index);
                if (startIndex !== -1) {
                    endIndex = text.indexOf("}}", startIndex + 2);
                }
                if (startIndex !== -1 && endIndex !== -1) {
                    if (startIndex !== index) {
                        parts.push(unescapeText(text.substring(index, startIndex)));
                    }
                    exp = text.substring(startIndex + 2, endIndex);
                    expFn = $parse(exp);
                    expressions.push(exp);
                    expressionFns.push(expFn);
                    expressionPositions.push(parts.length);
                    parts.push(expFn);
                    index = endIndex + 2;
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

        return $interpolate;
    }];
}