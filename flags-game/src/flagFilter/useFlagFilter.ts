import { useCallback, useEffect, useReducer } from "react";
import { FlagChecked } from "../flagsService/playList";
import { Actions } from "./actionsAndState";
import { initFlagFilter, reducer } from "./reducer";

export function useFlagFilter(flags: FlagChecked[] | null, selectedId: number) {
  const [state, dispatch] = useReducer(reducer, flags, initFlagFilter);

  useEffect(() => {
    if (flags && flags.length > 0) {
      if (selectedId !== state.selectedId) {
        dispatch({ type: Actions.ResetFilter, flags, selectedId });
      } else {
        dispatch({ type: Actions.SetFlags, flags });
      }
    }
  }, [flags, selectedId, state.selectedId]);

  const filterFlags = useCallback((searchText: string) => {
    dispatch({ type: Actions.ChangeFilter, searchText });
  }, []);

  return {
    filteredFlags: state.filteredFlags,
    filterFlags,
    searchText: state.searchText
  };
}
