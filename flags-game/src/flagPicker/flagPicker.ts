import { pickRandom, findValIndex } from "../common";
import { IGameState } from "./actionsAndState";

export function pickFlagInternal(state: IGameState) {
  const flagList = Object.keys(state.flagsState);
  const chances: number[] = [];
  let total = 0;
  for (const flag of flagList) {
    const stats = state.flagsState[flag];
    const chanceOldness = Math.min(state.numOfPlay - (stats.lastPlay || 0), flagList.length) / flagList.length;
    const wrong = Math.max(stats.wrong, 1);
    const correct = Math.max(stats.correct, 1);
    const z = Math.min(wrong, correct) - 1;
    const chanceWrong = (wrong - z) / (wrong + correct - 2 * z);
    const chanceTotal = (chanceOldness + chanceWrong) / 2;
    total += chanceTotal;
    chances.push(total);
  }
  const randomNum = pickRandom(total);
  const index = findValIndex(randomNum, chances, 0, chances.length);
  return flagList[index];
}
