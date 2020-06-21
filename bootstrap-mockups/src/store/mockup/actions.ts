import { MockupElementType } from "./models";
import { Action } from "redux";
import * as Mockup from "./actionNames";
import { MockupElement } from "./models";
import { createId } from "./internals";

export interface MockupElementAddition extends Action<typeof Mockup.Add> {
    parentId: number;
    index: number;
    newElement: MockupElement;
}

export interface MockupElementRemoval extends Action<typeof Mockup.Remove> {
    id: number;
}

export const MockupActions = {
    add(type: MockupElementType, parentId: number, index: number): MockupElementAddition {
        return {
            type: Mockup.Add,
            newElement: {
                id: createId(),
                type,
                children: []
            },
            index,
            parentId
        };
    },

    remove(id: number): MockupElementRemoval {
        return {
            type: Mockup.Remove,
            id
        };
    }
}
