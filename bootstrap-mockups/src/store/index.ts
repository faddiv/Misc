import { createStore, applyMiddleware, Store, compose } from "redux";
import thunk from "redux-thunk";
import { RootState, rootReducer } from "./RootStateAndReducer";


export function configureStore(initialState?: RootState) {

    let compose2 = compose;
    if (process.env.NODE_ENV !== "production") {
        compose2 = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;
    }

    const store = createStore(
        rootReducer
        , initialState
        , compose2(applyMiddleware(thunk))
    ) as Store<RootState>;

    return store;
}
