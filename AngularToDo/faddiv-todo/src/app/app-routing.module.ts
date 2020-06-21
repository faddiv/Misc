import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TodosComponent } from './todos/todos.component';
import { HomeComponent } from './home/home.component';
import { GoogleLoginGuard } from './user-management';


const routes: Routes = [
    {
        path: "todos",
        component: TodosComponent,
        canActivate: [GoogleLoginGuard]
    },
    {
        path: "",
        component: HomeComponent
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }
