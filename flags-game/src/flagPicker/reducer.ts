import { IGameFlagStateCollection, IGameState, PickFlagActions, Actions } from "./actionsAndState";
import { pickRandom } from "../common";

function pickFlagInternal(state: IGameState) {
  const flagList = Object.keys(state.flagsState);
  const maxValue = sumRatios(state);
  const randomNum = pickRandom(maxValue);
  let count = 0;
  for (const flag of flagList) {
    const stats = state.flagsState[flag];
    count += (stats.wrong + 1) / (stats.correct + 1);
    if (count >= randomNum) {
      return flag;
    }
  }
  return flagList[flagList.length - 1];
}

function sumRatios(state: IGameState) {
  let count = 0;
  const flagList = Object.keys(state.flagsState);
  for (const flag of flagList) {
    const stats = state.flagsState[flag];
    count += (stats.wrong + 1) / (stats.correct + 1);
  }
  return count;
}

export function initGameState(flags: string[]): IGameState {
  return {
    flags,
    lastFlag: flags[pickRandom(flags.length)],
    numOfPlay: 0,
    numOfCorrect: 0,
    numOfWrong: 0,
    flagsState: flags.reduce((flagCollection, flag) => {
      flagCollection[flag] = {
        correct: 0,
        numOfPlay: 0,
        wrong: 0
      }; return flagCollection
    }, {} as IGameFlagStateCollection)
  };
}

export function reducer(state: IGameState, action: PickFlagActions): IGameState {
  switch (action.type) {
    case Actions.IncrementCorrect:
      return {
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
    case Actions.IncrementWrong:
      return {
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
    case Actions.PickFlag:
      if (state.flags.length === 0)
        return state;
      return {
        ...state,
        lastFlag: pickFlagInternal(state)
      };
    case Actions.Reset:
      return initGameState(action.flags);
    default:
      return state;
  }
}
