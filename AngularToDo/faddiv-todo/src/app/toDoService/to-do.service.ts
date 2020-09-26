import { Injectable } from '@angular/core';
import { AngularFirestore, AngularFirestoreCollection } from '@angular/fire/firestore';
import { firestore } from 'firebase';
import { Observable } from 'rxjs';
import { AuthenticationService } from '../user-management';

type DateTime = Date;
type RecordIdentifier = string;
type UserIdentifier = string;

export interface INewToDoItem {
    text: string;
    checked: boolean;
    createdAt: firestore.Timestamp;
    checkedAt?: firestore.Timestamp;
}

export interface IToDoItem extends INewToDoItem {
    id: RecordIdentifier;
}

@Injectable({
    providedIn: 'root'
})
export class ToDoService {

    private _users: AngularFirestoreCollection<any>;
    private _todosCollection?: AngularFirestoreCollection<INewToDoItem>;
    private _todos: Observable<IToDoItem[]>;

    constructor(
        firebaseDb: AngularFirestore,
        authenticationService: AuthenticationService
    ) {
        this._users = firebaseDb.collection<INewToDoItem>("users");
        authenticationService.getUserObserver().subscribe(this.connect);
        this.connect(null);
    }

    get data(): Observable<IToDoItem[]> {
        return this._todos;
    }

    addToDo(todo: INewToDoItem) {
        this.validateLoggedIn();
        this._todosCollection.add(todo);
    }

    updateToDo(todo: IToDoItem) {
        this.validateLoggedIn();
        var doc = this._todosCollection.doc(todo.id);
        return doc.update({
            ...todo,
            id: undefined,
        });
    }

    private validateLoggedIn() {
        if (!this._todosCollection) {
            throw "User is not logged in";
        }
    }

    private connect = (user: firebase.User) => {
        if (user == null) {
            this.disconnect();
        } else {
            this._todosCollection = this._users.doc(`/${user.uid}`).collection("/todos");
            this._todos = this._todosCollection.valueChanges({
                idField: "id"
            });
            this._todos.subscribe((items) => {
                console.log("Items arrived:", items);
            })
        }
        return this.disconnect;
    }

    private disconnect = () => {
        this._todos = new Observable();
        this._todosCollection = null;
    }
}
