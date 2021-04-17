import { FlagInfo } from "./flagList";

export interface PlayList {
  id: number;
  name: string;
  flags: string[];
}

export interface FlagChecked extends FlagInfo {
  checked: boolean;
}

export interface PlayListWithFlag {
  id: number;
  name: string;
  invalids?: string[];
  flags: FlagChecked[];
}
