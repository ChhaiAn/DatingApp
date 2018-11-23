import { Component, OnInit } from '@angular/core';
import { AuthService } from '../service/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {} ;
  constructor(private authService: AuthService) { }

  ngOnInit() {
  }

  login() {
    this.authService.login(this.model).subscribe(
      next => {
        console.log('Login Success');
      },
      error => {
        console.log(error);
      }
    );
  }

  loggedIn() {
    const user = localStorage.getItem('Token');
    return !!user;
  }
  logout() {
    console.log("click");
    return localStorage.removeItem('Token');
  }
}
