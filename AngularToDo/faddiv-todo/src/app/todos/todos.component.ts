import { Component, OnInit } from '@angular/core';
import { ToDoService } from '../toDoService/to-do.service';

@Component({
    selector: 'app-todos',
    templateUrl: './todos.component.html',
    styleUrls: ['./todos.component.scss']
})
export class TodosComponent implements OnInit {

    constructor(
        private _toDoService: ToDoService
    ) {
    }
    tododod = "";
    public get todos() {
        return this._toDoService.data;
    }

    ngOnInit(): void {
    }

}
