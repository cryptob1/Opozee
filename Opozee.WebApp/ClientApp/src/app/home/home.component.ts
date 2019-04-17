import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';
import { Router, ActivatedRoute } from '@angular/router';
import { User } from '../_models';
import { UserService } from '../_services';
import { QuestionListing } from '../_models/question';

//import { Emoji } from 'emoji-mart'


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
    //this.userService.getUserRecords().pipe(first()).subscribe(users => {
    //  debugger;
    //  this.questionListing = users;
    //  console.log(this.questionListing);
    //});
  }

  searchForTag(hashtag) {
    this.router.navigateByUrl('/questionlistings/' + hashtag, { skipLocationChange: true }).then(() =>
      this.router.navigate(['/questions/', hashtag]));
    //this.router.navigate(['/questions'], { queryParams: { tag: 1 } });
  }

}
