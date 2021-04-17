import { Action } from "../common";

export enum Actions {
  IncrementCorrect = "flagPicker.IncrementCorrect",
  IncrementWrong = "flagPicker.IncrementWrong",
  PickFlag = "flagPicker.PickFlag",
  Reset = "flagPicker.Reset",
}

export interface IncrementCorrectAction extends Action<typeof Actions.IncrementCorrect> {
  flag: string;
}

export interface IncrementWrongAction extends Action<typeof Actions.IncrementWrong> {
  flag: string;
}

export interface PickFlagAction extends Action<typeof Actions.PickFlag> {

}

export interface ResetAction extends Action<typeof Actions.Reset> {
  flags: string[];
}


export type PickFlagActions = IncrementCorrectAction | IncrementWrongAction | PickFlagAction | ResetAction;

export interface IGameFlagState {
  correct: number;
  wrong: number;
  numOfPlay: number;
}

export interface IGameFlagStateCollection {
  [name: string]: IGameFlagState;
}

export interface IGameState {
  flags: string[];
  lastFlag: string;
  numOfPlay: number;
  numOfWrong: number;
  numOfCorrect: number;
  flagsState: IGameFlagStateCollection;
}
