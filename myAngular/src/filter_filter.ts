import * as _ from 'lodash';
import { IComparator } from "./angularInterfaces";

"use strict";
export function filterFilter() {
    return function (array: any[], filterExpr: any, comparator: IComparator | boolean) {
        var predicateFn: Function;
        if (filterExpr instanceof Function) {
            predicateFn = filterExpr;
        } else if (typeof (filterExpr) === "undefined"
            || filterExpr === null
            || _.isString(filterExpr)
            || _.isNumber(filterExpr)
            || _.isBoolean(filterExpr)
            || _.isObject(filterExpr)) {
            predicateFn = createPredicateFn(filterExpr, comparator);
        } else {
            return array;
        }
        return _.filter(array, predicateFn);
    }
}
function createPredicateFn(expression: any, comparator: IComparator | boolean): Function {
    var shouldMatchPrimitives =
        _.isObject(expression) && ("$" in expression);
    if(comparator === true) {
        comparator = _.isEqual;
    } else if (!_.isFunction(comparator)) {
        comparator = function (actual: string, expected: string) {
            if (_.isUndefined(actual)) {
                return false;
            }
            if (actual === null || expected === null) {
                return actual === expected;
            }
            actual = ("" + actual).toLowerCase();
            expected = ("" + expected).toLowerCase();
            return actual.indexOf(expected) !== -1;
        }
    }
    function deepCompare(actual: any, expected: string, comparator, matchAnyProperty: boolean, inWildcard?: boolean) {
        if (_.isString(expected) && _.startsWith(expected, "!")) {
            return !deepCompare(actual, expected.substring(1), comparator, matchAnyProperty);
        }
        if (_.isArray(actual)) {
            return _.some(actual, function (actualItem) {
                return deepCompare(actualItem, expected, comparator, matchAnyProperty);
            });
        }
        if (_.isObject(actual)) {
            if (_.isObject(expected) && !inWildcard) {
                return _.every(_.toPlainObject<_.Dictionary<any>>(expected),
                    function (expectedVal, expectedKey) {
                        if (_.isUndefined(expectedVal)) {
                            return true;
                        }
                        var isWildcard = (expectedKey === "$");
                        var actualVal = isWildcard ? actual : actual[expectedKey];
                        return deepCompare(actualVal, expectedVal, comparator, isWildcard, isWildcard);
                    }
                );
            } else if (matchAnyProperty) {
                return _.some(actual, (value: any) => {
                    return deepCompare(value, expected, comparator, matchAnyProperty);
                });
            } else {
                return comparator(actual, expected);
            }
        } else {
            return comparator(actual, expected);
        }
    }
    return function predicateFn(item: string) {
        if (shouldMatchPrimitives && !_.isObject(item)) {
            return deepCompare(item, expression.$, comparator, false);
        }
        return deepCompare(item, expression, comparator, true);
    }
}
