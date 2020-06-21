import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../user-management';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  constructor(
      private _auth: AuthenticationService,
      private _route: Router
  ) { }

  ngOnInit() {
    this._auth.getUserObserver().subscribe((user) => {
        if(user != null) {
            this._route.navigateByUrl("/todos");
        }
    });

  }

}
