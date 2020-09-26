import { Component, OnInit } from '@angular/core';
import { ToDoService, IToDoItem } from '../toDoService/to-do.service';
import { faCheck } from '@fortawesome/free-solid-svg-icons';

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

    icon = {
      faCheck
    }

    public get todos() {
        return this._toDoService.data;
    }

    ngOnInit(): void {
    }

    check(todo: IToDoItem) {
      todo.checked = !todo.checked;
    }
}
