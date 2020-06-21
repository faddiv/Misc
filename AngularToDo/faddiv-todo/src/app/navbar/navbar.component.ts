import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../user-management';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.component.html',
    styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {

    isMenuCollapsed = true;

    user: Observable<firebase.User>;

    constructor(
        private authenticationService: AuthenticationService
    ) {
        this.user = authenticationService.getUserObserver();
    }
    toggleNavbar() {
        this.isMenuCollapsed = !this.isMenuCollapsed;
    }

    closeNavbar() {
        this.isMenuCollapsed = true;
    }

    login() {
        this.authenticationService.login();
    }

    logout() {
        this.authenticationService.logout();
    }
}
