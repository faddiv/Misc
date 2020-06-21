import { combineReducers } from "redux";
import { MockupElement, mockupReducers as container } from "./mockup";

export interface RootState {
    container: MockupElement;
}

export const rootReducer = combineReducers<RootState>({
    container
});
