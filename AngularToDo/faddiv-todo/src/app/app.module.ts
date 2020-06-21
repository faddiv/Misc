import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AngularFireModule, FirebaseOptions } from "@angular/fire";
import { AngularFirestoreModule } from "@angular/fire/firestore";
import { AngularFireAuthModule } from '@angular/fire/auth';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import * as firebaseConfig from "../secrets.json";
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NavbarComponent } from './navbar/navbar.component';
import { TodosComponent } from './todos/todos.component';
import { HomeComponent } from './home/home.component';
import { ToDoService } from './toDoService/to-do.service';
import { UserManagementModule } from './user-management';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    TodosComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    AngularFireModule.initializeApp(firebaseConfig["default"]),
    AngularFirestoreModule.enablePersistence(),
    NgbModule,
    UserManagementModule
  ],
  providers: [ToDoService],
  bootstrap: [AppComponent]
})
export class AppModule { }
