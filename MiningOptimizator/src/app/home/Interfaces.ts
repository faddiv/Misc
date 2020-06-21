export interface IMiner extends IEquatable<IMiner>, IPrintable {
  readonly cells: ICell[];
  readonly canPlaceDown: boolean;
  readonly influencedCells: ICell[];
  readonly placed: boolean;
  place(down: boolean): boolean;
}

export interface ICell extends IEquatable<ICell> {
  miner: IMiner;
  isMined: boolean;
  hasMine: boolean;
  readonly col: number;
  readonly row: number;
  readonly up?: ICell;
  readonly down?: ICell;
  readonly left?: ICell;
  readonly right?: ICell;
  readonly canHaveMiner: boolean;
  reset(): void;
}

export interface IEquatable<T> {
  equals(other: T): boolean;
}

export interface IComparable<T> {
  compareTo(other: T): number;
}

export interface IPlacement extends IComparable<IPlacement>, IPrintable {
  miners: IMiner[];
  point: number;
  applied: boolean;
  apply(): void;
}
export interface IPrintable {
  toString(): string;
}
