import { zipArray, flat, distinct, pushRepeat } from './helpers';
import { IMiner, ICell } from './Interfaces';

export class Miner implements IMiner {
  placed: boolean;
  colTopLeft: number;
  rowTopLeft: number;
  colDownRigth: number;
  rowDownRigth: number;
  influencedCells: ICell[];
  constructor(
    public cells: ICell[]) {
    cells.sort(minerCellSort);
    validateCells(cells);
    this.placed = false;
    this.colTopLeft = cells[0].col;
    this.rowTopLeft = cells[0].row;
    this.colDownRigth = cells[3].col;
    this.rowDownRigth = cells[3].row;
    this.influencedCells = collectInfluencedCells(this);
  }

  get canPlaceDown() {
    if (this.placed)
      return true;
    if (this.cells.some(cell => cell.miner !== undefined && cell.miner !== this))
      return false;
    if (hasMinedSpot(this.influencedCells)) {
      return false;
    }
    return true;
  }

  place(down: boolean) {
    if (this.placed == down)
      return false;
    if (down) {
      if (!this.canPlaceDown) {
        return false;
      }
      for (const cell of this.cells) {
        cell.miner = this;
      }
    } else {
      for (const cell of this.cells) {
        cell.miner = undefined;
      }
    }
    for (const cell of this.influencedCells) {
      cell.isMined = down;
    }
    this.placed = down;
    return true;
  }

  equals(other: Miner) {
    return zipArray(this.cells, other.cells)
      .every(arrs => arrs[0].equals(arrs[1]));
  }

  toString() {
    return `${this.colTopLeft}x${this.rowTopLeft} - ${this.colDownRigth}x${this.rowDownRigth}`;
  }
}

function minerCellSort(cellLeft: ICell, cellRight: ICell) {
  if (cellLeft.row > cellRight.row)
    return 1;
  if (cellLeft.row < cellRight.row)
    return -1;
  if (cellLeft.col > cellRight.col)
    return 1;
  if (cellLeft.col < cellRight.col)
    return -1;
  return 0;
}

function validateCells(cells: ICell[]) {
  if (cells.length != 4) {
    throw new Error("Miner must have 4 cells");
  }
  if (cells[0].row !== cells[1].row) {
    throw new Error("Miner must have two upper cells");
  }
  if (cells[2].row !== cells[3].row) {
    throw new Error("Miner must have two lower cells");
  }
  if (cells[0].col !== cells[2].col) {
    throw new Error("Miner must have two left cells");
  }
  if (cells[1].col !== cells[3].col) {
    throw new Error("Miner must have two right cells");
  }
}

export function getPossibleMiners(cell: ICell) {
  var miners: Miner[] = [];
  if (cell.up) {
    miners.push(...getPossibleMinersLR(cell, cell.up));
  }
  if (cell.down) {
    miners.push(...getPossibleMinersLR(cell.down, cell));
  }
  return miners;
}

function getPossibleMinersLR(cellDown: ICell, cellUp: ICell) {
  var miners: Miner[] = [];
  if (cellDown.left && cellUp.left) {
    var cells = [cellUp.left, cellUp, cellDown.left, cellDown];
    if (noMine(cells)) {
      miners.push(new Miner(cells));
    }
  }
  if (cellDown.right && cellUp.right) {
    var cells = [cellUp, cellUp.right, cellDown, cellDown.right];
    if (noMine(cells)) {
      miners.push(new Miner(cells));
    }
  }
  return miners;
}

function noMine(cells: ICell[]) {
  return cells.every(cell => cell.isEmpty);
}

export function getMiningLocations(cells: ICell[][]) {
  var mineBorderCells = flat(cells)
    .filter(cell => cell.canHaveMiner);
  var miners = flat(mineBorderCells.map(cell => getPossibleMiners(cell)));
  miners = distinct(miners, (a, b) => a.equals(b));
  return miners;
}

export function collectInfluencedCells(miner: IMiner) {
  var neighbours: ICell[] = [];
  pushRepeat(neighbours, miner.cells[0], 2, cell => cell.up);
  pushRepeat(neighbours, miner.cells[2], 2, cell => cell.down);
  var startingCells = [...neighbours];
  for (const startCell of startingCells) {
    pushRepeat(neighbours, startCell, 2, cell => cell.left);
    pushRepeat(neighbours, startCell, 3, cell => cell.right);
  }
  pushRepeat(neighbours, miner.cells[0], 2, cell => cell.left);
  pushRepeat(neighbours, miner.cells[2], 2, cell => cell.left);
  pushRepeat(neighbours, miner.cells[1], 2, cell => cell.right);
  pushRepeat(neighbours, miner.cells[3], 2, cell => cell.right);
  return neighbours.filter(cell => cell.hasMine);
}

function hasMinedSpot(cells: ICell[]) {
  return cells.some(cell => cell.isMined)
}

export function takeAllMinerUp(miners:IMiner[]) {
  miners.forEach(miner => miner.place(false));
}
