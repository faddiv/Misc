import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthenticationService } from './authentication/authentication.service';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AngularFireAuthModule } from '@angular/fire/auth';



@NgModule({
    declarations: [],
    imports: [
        CommonModule,
        AngularFireAuthModule
    ],
    providers: [AuthenticationService]
})
export class UserManagementModule { }
