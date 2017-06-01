import { IScopeInternal } from "../src/angularInterfaces";

interface IScopeEx extends IScopeInternal {

    //Test properties
    name?: string;
    text?: string;
    number?: number;
    number2?: number;
    counter?: number;
    counter2?: number;
    asyncApplied?: boolean;
    initial?: string;
    nameUpper?: string;
    array?: number[];
    args?: IArguments;
    nodeList?: NodeListOf<HTMLElement>;
    obj?: any;
    asyncEvaluated?: boolean;
    asyncEvaluatedImmediatelly?: boolean;
    asyncEvaluatedTimes?: number;
    watchedWalue?: string;
    postDigestDone?: boolean;
    aFunction?: Function;
}