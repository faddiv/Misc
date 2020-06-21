import { ICell, IMiner } from './Interfaces';

type CellCss = 'empty' | 'mine' | 'miner' | 'neighbour';

function addIfDefined<T>(arr: T[], elm?: T) {
  if (elm) {
    arr.push(elm);
  }
}

export class CellType implements ICell {

  hasMine: boolean;
  miner: IMiner;
  up?: CellType;
  down?: CellType;
  left?: CellType;
  right?: CellType;

  constructor(
    public readonly col: number,
    public readonly row: number
  ) {
    this.hasMine = false;
  }

  get neighbours() {
    var n: CellType[] = [];
    addIfDefined(n, this.up);
    addIfDefined(n, this.down);
    addIfDefined(n, this.left);
    addIfDefined(n, this.right);
    return n;
  }

  public css(): CellCss {
    if (!this.hasMine) {
      if (this.miner) {
        return "miner";
      }
      if (this.hasMineNeighbour) {
        return "neighbour";
      }
      return "empty";
    }
    return "mine";
  }

  get hasMineNeighbour() {
    for (const n of this.neighbours) {
      if (n.hasMine) {
        return true;
      }
    }
    return false;
  }

  connectLeft(left: CellType) {
    this.left = left;
    if (left) {
      left.right = this;
    }
  }

  connectUp(up: CellType) {
    this.up = up;
    if (up) {
      up.down = this;
    }
  }

  get canHaveMiner() {
    return !this.hasMine && this.hasMineNeighbour;
  }

  equals(other: CellType) {
    return this.col === other.col && this.row === other.row;
  }
}
