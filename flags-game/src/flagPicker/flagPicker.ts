import { pickRandom } from "../common";
import { IGameState } from "./actionsAndState";

function sumRatios(state: IGameState) {
  let count = 0;
  const flagList = Object.keys(state.flagsState);
  for (const flag of flagList) {
    const stats = state.flagsState[flag];
    count += (stats.wrong + 1) / (stats.correct + 1);
  }
  return count;
}

export function pickFlagInternal(state: IGameState) {
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
