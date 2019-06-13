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

  //earnings30: UserEarnModel[] = [];
  //earnings7: UserEarnModel[] = [];
  //earnings1: UserEarnModel[] = [];
  earnings: {
    [id: number]: UserEarnModel[];
  } = {};

  constructor(
    private route: ActivatedRoute,
    private userService: UserService)
  {

    
  }

  ngOnInit() {
    this.getTopEarners(3)
    this.getTopEarners(7)
    this.getTopEarners(30)
  }





  private getTopEarners(days: number) {



    this.userService.getTopEarners(days).subscribe(data => {

      
      this.earnings[days] = (data as UserEarnModel[]);
      //this.earnings.push({key: days, value: (data as UserEarnModel[]) });
      

    },
      error => {
        console.log('error');
      });
  }



}
