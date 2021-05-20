import { pickRandom } from "../common";
import { extTable } from "../common/extendedStore";
import { PlayList } from "../flagsService/playList";
import { IGameFlagStateCollection, IGameState } from "./actionsAndState";
import { pickFlagInternal } from "./flagPicker";

const table = extTable<IGameState>("GameState");

function flagsNotChanged(savedState: IGameState, playList: PlayList) {
  if (playList.flags.length !== savedState.flags.length)
    return false;
  const set1 = new Set(playList.flags);
  for (const flag2 of savedState.flags) {
    if (!set1.has(flag2))
      return false;
  }
  return true;
}

export function getStateForPlay(playList: PlayList | null): IGameState {
  playList = playList || {
    id: 0, name: "", flags: []
  };
  const savedState = table.getElementByKey(playList.id);
  if (savedState !== null && flagsNotChanged(savedState, playList)) {
    savedState.lastFlag = pickFlagInternal(savedState);
    return savedState;
  }
  return {
    id: playList.id,
    version: 1,
    flags: [...playList.flags],
    lastFlag: playList.flags[pickRandom(playList.flags.length)],
    numOfPlay: 0,
    numOfCorrect: 0,
    numOfWrong: 0,
    flagsState: playList.flags.reduce((flagCollection, flag) => {
      flagCollection[flag] = {
        correct: 0,
        numOfPlay: 0,
        wrong: 0
      }; return flagCollection
    }, {} as IGameFlagStateCollection)
  }
}

export function saveStateOfPlay(gameState: IGameState) {
  table.saveElement(gameState);
}
