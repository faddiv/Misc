import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { CellType } from './CellType';
import { lastItem, forEach2D } from "./helpers";
import { distinctUntilChanged } from "rxjs/operators";
import { Miner, getMiningLocations, takeAllMinerUp } from './Miner';
import { traversePlacements, resetAll } from './AI';
import { IPlacement } from './Interfaces';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  options = new FormGroup({
    columnCount: new FormControl(50),
    rowCount: new FormControl(50),
  });

  cells: CellType[][];

  miners: Miner[];

  placements: IPlacement[];

  mouseOperation?: boolean;

  constructor() {
  }

  ngOnInit(): void {
    this.initCells();
    this.options.get("rowCount").valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(this.resize);
    this.options.get("columnCount").valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(this.resize);
  }

  resize = () => {
    this.initCells();
    this.calcMiners();
  }

  reset = () => {
    forEach2D(this.cells, cell => {
      cell.reset();
    });
    this.placements = [];
    this.miners = [];
  }

  traverse() {
    takeAllMinerUp(this.miners);
    this.placements = traversePlacements(this.miners);
  }

  get inPainting() {
    return this.mouseOperation !== undefined;
  }
  get columnCount() {
    return this.options.get("columnCount").value as number;
  }

  get rowCount() {
    return this.options.get("rowCount").value as number;
  }

  get gtcStyle() {
    return `repeat(${this.columnCount}, 20px)`;
  }

  cell = (col: number, row: number, hasMine?: boolean) => {
    if (typeof hasMine !== "undefined") {
      this.cells[row][col].hasMine = hasMine;
    }
    if (row < this.cells.length) {
      if (col < this.cells[row].length) {
        return this.cells[row][col];
      } else {
        console.log(`Hit default row: index: ${col} should:${this.columnCount} real:${this.cells.length}`);
      }
    } else {
      console.log(`Hit default row: index: ${row} should:${this.rowCount} real:${this.cells[col].length}`);
    }
  }

  startDraw(evt: MouseEvent, cell: CellType) {
    if (this.inPainting) {
      return;
    }
    if (evt.button !== 0) {
      return;
    }
    evt.preventDefault();
    this.mouseOperation = !cell.hasMine;
    cell.hasMine = this.mouseOperation;
    this.calcMiners();
  }

  applyPlacements(placement: IPlacement) {
    var applied = placement.applied;
    takeAllMinerUp(this.miners);
    resetAll(this.placements);
    if (applied)
      return;
    placement.apply();
  }

  drawing(evt: MouseEvent, cell: CellType) {
    if (!this.inPainting) {
      return;
    }
    if (cell.hasMine !== this.mouseOperation) {
      cell.hasMine = this.mouseOperation;
      this.calcMiners();
      evt.preventDefault();
    }
  }

  endDraw(evt: MouseEvent) {
    if (!this.inPainting) {
      return;
    }
    this.mouseOperation = undefined;
  }

  private initCells() {
    if (this.cells === undefined) {
      this.cells = [];
    }
    while (this.cells.length < this.rowCount) {
      var row: CellType[] = [];
      this.adjustRow(row, this.cells.length, lastItem(this.cells));
      this.cells.push(row);
    }
    if (this.cells.length > this.rowCount) {
      this.cells.splice(this.rowCount);
      var lastRow = lastItem(this.cells);
      if (lastRow) {
        for (const cell of lastRow) {
          cell.down = undefined;
        }
      }
    }
    for (let rowIndex = 0; rowIndex < this.cells.length; rowIndex++) {
      const row = this.cells[rowIndex];
      this.adjustRow(row, rowIndex, rowIndex > 0 ? this.cells[rowIndex - 1] : undefined);
    }
    console.log("Cells initialized");
  }

  private calcMiners() {
    this.miners = getMiningLocations(this.cells);
  }

  private adjustRow(row: CellType[], rowIndex: number, upRow?: CellType[]) {
    while (row.length < this.columnCount) {
      var newCell = new CellType(row.length, rowIndex);
      var left = lastItem(row);
      newCell.connectLeft(left);
      if (upRow) {
        newCell.connectUp(upRow[row.length]);
      }
      row.push(newCell);
    }
    if (row.length > this.columnCount) {
      row.splice(this.columnCount);
      if (row.length > 0) {
        lastItem(row).right = undefined;
      }
    }
  }
}
