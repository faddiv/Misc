"use strict";
import { IParseService, ICompiledExpression, IInterpolationFunction } from "angular";
import * as _ from "lodash";


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
                    parts.push(expFn);
                    expressions.push(exp);
                    index = endIndex + 2;
                } else {
                    parts.push(unescapeText(text.substring(index)));
                    break;
                }
            }
            if (expressions.length || !mustHaveExpressions) {
                return _.extend(function interpolationFn(context: any) {
                    return _.reduce<string | ICompiledExpression, string>(parts, function (result, part) {
                        if (_.isFunction(part)) {
                            return result + stringify(part(context));
                        } else {
                            return result + part;
                        }
                    }, "");
                }, {
                    expressions: expressions
                });
            }
        }

        return $interpolate;
    }];
}