import { IGameFlagStateCollection, IGameState, PickFlagActions, Actions } from "./actionsAndState";
import { pickRandom } from "../common";
import { getStateForPlay, saveStateOfPlay } from "./gameStateStore";
import { PlayList } from "../flagsService/playList";
import { pickFlagInternal } from "./flagPicker";

export function initGameState(playList: PlayList | null): IGameState {
  return getStateForPlay(playList);
}

export function reducer(state: IGameState, action: PickFlagActions): IGameState {
  let newState = state;
  switch (action.type) {
    case Actions.IncrementCorrect:
      newState = {
        id: state.id,
        version: state.version,
        flags: state.flags,
        numOfPlay: state.numOfPlay + 1,
        numOfWrong: state.numOfWrong,
        numOfCorrect: state.numOfCorrect + 1,
        lastFlag: state.lastFlag,
        flagsState: {
          ...state.flagsState,
          [action.flag]: {
            numOfPlay: state.flagsState[action.flag].numOfPlay + 1,
            correct: state.flagsState[action.flag].correct + 1,
            wrong: state.flagsState[action.flag].wrong,
          }
        }
      };
      break;
    case Actions.IncrementWrong:
      newState = {
        id: state.id,
        version: state.version,
        flags: state.flags,
        numOfPlay: state.numOfPlay + 1,
        numOfWrong: state.numOfWrong + 1,
        numOfCorrect: state.numOfCorrect,
        lastFlag: state.lastFlag,
        flagsState: {
          ...state.flagsState,
          [action.flag]: {
            numOfPlay: state.flagsState[action.flag].numOfPlay + 1,
            correct: state.flagsState[action.flag].correct,
            wrong: state.flagsState[action.flag].wrong + 1,
          }
        }
      };
      break;
    case Actions.PickFlag:
      if (state.flags.length === 0)
        return state;
      return {
        ...state,
        lastFlag: pickFlagInternal(state)
      };
    case Actions.Reset:
      return initGameState(action.playList);
    default:
      return state;
  }
  saveStateOfPlay(newState);
  return newState;
}
