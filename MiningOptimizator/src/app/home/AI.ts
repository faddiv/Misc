import { ICell, IMiner, IPlacement } from "./Interfaces";
import { subArray } from './helpers';

class Placement implements IPlacement {
  applied = false;

  constructor(
    public miners: IMiner[],
    public point: number
  ) {

  }

  apply() {
    if (this.applied)
      return;
    this.miners.forEach(miner => miner.place(true));
    this.applied = true;
  }

  compareTo(other: IPlacement): number {
    var result = this.point - other.point;
    if (result !== 0)
      return result;
    return other.miners.length - this.miners.length;
  }

  toString() {
    return `point:${this.point} miners:${this.miners.length}`;
  }
}

function calcPoint(miner: IMiner, basePoint = 0) {
  return miner.influencedCells.length + basePoint;
}

export function traversePlacements(miners: IMiner[]): IPlacement[] {
  var placements = backtrack(miners, undefined);
  placements.sort((a, b) => -a.compareTo(b));
  return placements;
}


function backtrack(miners: IMiner[], parent: Placement) {
  var results: Placement[] = [];
  for (let index = 0; index < miners.length; index++) {
    const miner = miners[index];
    var placed = miner.place(true);
    if (!placed) {
      continue;
    }
    var placement = new Placement(parent?.miners ? [...parent?.miners, miner] : [miner], calcPoint(miner, parent?.point));
    var subList = miners.filter((miner, i) => i > index && miner.canPlaceDown);
    if (subList.length > 0) {
      var placements = backtrack(subList, placement);
      results.push(...placements);
    } else {
      results.push(placement);
    }
    miner.place(false);
  }
  return results;
}

export function resetAll(placements: IPlacement[]) {
  placements.forEach(placement => placement.applied = false);
}
