import { Action } from "../common";
import { FlagChecked } from "../flagsService/playList";

export enum Actions {
  ChangeFilter = "flagFilter.ChangeFilter",
  SetFlags = "flagFilter.SetFlags",
  ResetFilter = "flagFilter.ResetFilter",
}

export interface ChangeFilterAction extends Action<typeof Actions.ChangeFilter> {
  searchText: string;
}

export interface SetFlagsAction extends Action<typeof Actions.SetFlags> {
  flags: FlagChecked[];
}

export interface ResetFilterAction extends Action<typeof Actions.ResetFilter> {
  flags: FlagChecked[] | null;
  selectedId: number;
}


export type FlagFilterActions = ChangeFilterAction | SetFlagsAction | ResetFilterAction;

export interface IFlagFilterState {
  searchText: string;
  flags: FlagChecked[];
  filteredFlags: FlagChecked[];
  selectedId: number;
}
