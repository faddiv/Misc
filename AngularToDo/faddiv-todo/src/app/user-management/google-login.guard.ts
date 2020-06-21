import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthenticationService } from './authentication/authentication.service';

@Injectable({
    providedIn: 'root'
})
export class GoogleLoginGuard implements CanActivate {

    constructor(
        private _authenticationService: AuthenticationService
    ) {

    }

    canActivate(
        next: ActivatedRouteSnapshot,
        state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
            return this._authenticationService.getUserObserver()
                .pipe(map(this.isUserLoggedIn));
    }

    private isUserLoggedIn = (user: firebase.User) => {
        return !!user;
    }
}
