import { Injectable } from '@angular/core';
import { AngularFireAuth } from "@angular/fire/auth";
import { Observable } from "rxjs";
import { auth } from "firebase/app";

@Injectable({
    providedIn: 'root'
})
export class AuthenticationService {
    private user: Observable<firebase.User>;

    constructor(
        private firebaseAuth: AngularFireAuth
    ) {
        this.user = firebaseAuth.authState
    }

    public getUserObserver() {
        return this.user;
    }

    public login(): Promise<void> {
        return this.firebaseAuth.signInWithRedirect(new auth.GoogleAuthProvider());
    }

    public logout() {
        return this.firebaseAuth.signOut();
    }
}
