import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';
import { Router, ActivatedRoute } from '@angular/router';
import { User } from '../_models';
import { UserService } from '../_services';
import { QuestionListing } from '../_models/question';

@Component({ templateUrl: 'home.component.html' })
export class HomeComponent implements OnInit {
  currentUser: User;
  users: User[] = [];
  questionListing: QuestionListing[] = [];
  

  constructor(private userService: UserService,private route: ActivatedRoute,
    private router: Router) {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
  }

  ngOnInit() {
    debugger;
    //this.loadAllUsers();
    this.getUserALLRecords();
  }

  deleteUser(id: number) {
    //this.userService.delete(id).pipe(first()).subscribe(() => { 
    // this.loadAllUsers() 
    //});
  }

  //seemes to be getting popular questions
  private getUserALLRecords() {
    debugger;
    this.userService.getUserRecords().pipe(first()).subscribe(users => {
      debugger;
      this.questionListing = users;
      console.log(this.questionListing);
    });
  }




}
