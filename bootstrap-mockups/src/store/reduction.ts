export function simpleReducer<TState>(state: TState, action: () => TState | undefined) {
    let newState = action();
    if (typeof newState !== "undefined" && newState !== state) {
        return {
            ...state,
            ...newState
        };
    } else {
        return state;
    }
}