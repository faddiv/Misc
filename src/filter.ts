import { filterFilter } from './filter_filter';
import * as _ from 'lodash';
import { IServiceProvider, IFilterFactory, IFilter } from "./angularInterfaces";
import { auto } from 'angular';
"use strict";
export default class $FilterProvider implements IServiceProvider {

    private static $inject = ["$provide"];
    constructor(private $provide: auto.IProvideService) {
        this.register("filter", filterFilter);
    }

    public register(name: string | _.Dictionary<IFilterFactory>, factory?: IFilterFactory) {
        if (_.isString(name)) {
            this.$provide.factory(name + "Filter", factory);
        } else {
            return _.map(name, (factory: IFilterFactory, name: string) => {
                return this.register(name, factory);
            });
        }
    }
    $get = ["$injector", function ($injector: auto.IInjectorService) {
        return function filter(name: string): IFilter {
            return $injector.get<IFilter>(name + "Filter");
        }
    }];
}

