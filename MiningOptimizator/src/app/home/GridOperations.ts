import { CellType } from "./CellType";
import { flat, distinct } from './helpers';
import { getPossibleMiners } from './Miner';

export function getMiningLocations(cells: CellType[][]) {
  var mineBorderCells = flat(cells)
    .filter(cell => cell.canHaveMiner);
  var miners = flat(mineBorderCells.map(cell => getPossibleMiners(cell)));
  miners = distinct(miners, (a, b) => a.equals(b));
  return miners;
}
