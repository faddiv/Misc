import { FlagChecked } from "../flagsService/playList";
import { Actions, FlagFilterActions, IFlagFilterState } from "./actionsAndState";

export function initFlagFilter(flags: FlagChecked[] | null, selectedId: number = -1): IFlagFilterState {
  if (!flags) {
    flags = [];
  }
  return {
    searchText: "",
    flags,
    filteredFlags: flags,
    selectedId
  };
}

function filterFlags(flags:FlagChecked[], searchText: string) {
  var lower = searchText.toLowerCase();
  return flags.filter(e => e.Hun.toLowerCase().indexOf(lower) !== -1);
}

export function reducer(state: IFlagFilterState, action: FlagFilterActions): IFlagFilterState {
  switch (action.type) {
    case Actions.ChangeFilter:
      return {
        searchText: action.searchText,
        flags: state.flags,
        selectedId: state.selectedId,
        filteredFlags: filterFlags(state.flags, action.searchText),
      };
    case Actions.ResetFilter:
      return initFlagFilter(action.flags, action.selectedId);

    case Actions.SetFlags:
      return {
        ...state,
        flags: action.flags,
        filteredFlags: filterFlags(action.flags, state.searchText)
      };
    default:
      return state;
  }
}
