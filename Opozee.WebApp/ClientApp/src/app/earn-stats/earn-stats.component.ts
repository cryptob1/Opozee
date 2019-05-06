import { Component, OnInit, ViewChild, EventEmitter } from '@angular/core';
import { Location } from "@angular/common";
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
 
import { UserService } from '../_services/user.service';
import { first } from 'rxjs/operators';
import {  UserEarnModel } from '../_models/user';
import { PostQuestionDetail, BookMarkQuestionVM } from '../_models/user';
import { debounce } from 'rxjs/operator/debounce';
 

@Component({
  selector: 'app-earn-stats',
  templateUrl: './earn-stats.component.html',
  styleUrls: ['./earn-stats.component.css']
})
export class EarnStatsComponent implements OnInit {

  earnings: UserEarnModel[] = [];

  constructor(
    private route: ActivatedRoute,
    private userService: UserService)
  {

    
  }

  ngOnInit() {
    this.getTopEarners(30)
  }





  private getTopEarners(days: number) {



    this.userService.getTopEarners(days).subscribe(data => {
       
      this.earnings = data as UserEarnModel[];
      console.log(this.earnings);

    },
      error => {
        console.log('error');
      });
  }



}