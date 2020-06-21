import { Reducer } from "redux";
import { MockupElement } from "./models";
import * as Mockup from "./actionNames";
import { MockupElementAddition, MockupElementRemoval } from "./actions";
import { initialState, addElement, removeElement } from "./internals";
import { simpleReducer } from "../reduction";

type MockupActionParams = MockupElementAddition | MockupElementRemoval;

export const mockupReducers: Reducer<MockupElement, MockupActionParams> = (
    state = initialState,
    action
) => simpleReducer(state, () => {
    switch (action.type) {
        case Mockup.Add:
            // TODO implement
            return addElement(state, action.newElement, action.parentId, action.index);
        case Mockup.Remove:
            // TODO implement
            return removeElement(state, action.id);
    }
});