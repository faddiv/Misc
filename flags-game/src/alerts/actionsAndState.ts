import { Action } from "../common";

export enum Actions {
  Show = "alerts.Show",
  Hide = "alerts.Hide"
}

export interface ShowAction extends Action<typeof Actions.Show> {
  variant: string;
  message: string;
}

export interface HideAction extends Action<typeof Actions.Hide> {

}

export type AlertActions = ShowAction | HideAction;

export interface AlertState {
  show: boolean;
  variant: string;
  message: string;
}
