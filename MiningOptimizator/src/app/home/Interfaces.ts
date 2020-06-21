export interface IMiner extends IEquatable<IMiner> {
}
export interface ICell extends IEquatable<ICell> {
  miner: IMiner;
  readonly col: number;
  readonly row: number;
  up?: ICell;
  down?: ICell;
  left?: ICell;
  right?: ICell;
  hasMine: boolean;
}

export interface IEquatable<T> {
  equals(other: T): boolean;
}
