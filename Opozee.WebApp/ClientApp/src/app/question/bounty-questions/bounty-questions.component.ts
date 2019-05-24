import { Component, OnInit, ViewChild, EventEmitter } from '@angular/core';
import { Location } from "@angular/common";
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { first } from 'rxjs/operators';
import { debounce } from 'rxjs/operator/debounce';
import { UserService } from '../../_services';
import { LocalStorageUser } from '../../_models';
import { AppConfigService } from '../../appConfigService';


@Component({
  selector: 'app-bounty-questions',
  templateUrl: './bounty-questions.component.html',
  styleUrls: ['./bounty-questions.component.css']
})
export class BountyQuestionsComponent implements OnInit {
  bountyQuestions: any;
  loading: boolean = false;
  localStorageUser: LocalStorageUser;
  startDate: any;
  endDate: any;

  constructor(private route: ActivatedRoute, private userService: UserService, private configService: AppConfigService) {

    this.startDate = this.configService.bountyStartDate;
    this.endDate = this.configService.bountyEndDate;

    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
  }

  ngOnInit() {

    this.getBountyQuestions(this.startDate, this.endDate);
  }

  getBountyQuestions(startDate?, endDate?) {
    this.loading = true;
    this.userService.getBountyQuestions(startDate, endDate)
      .subscribe(data => {
        this.bountyQuestions = data;
        this.loading = false;
      },
        error => {
          this.loading = false;
          console.log('GetBountyQuestions err', error);
        });
  }

  onStartDateChange(event, date) {
    console.log('event', event.target.value,event)
    if (date == 'start') {
      this.startDate = event.target.value;
    } else if (date == 'end') {
      this.endDate = event.target.value;
    }
    console.log('startDate - endDate', this.startDate, this.endDate)
  }

  filterBQ() {
    console.log('startDate - endDate', this.startDate, this.endDate)
    if (this.startDate && this.endDate) {
      this.getBountyQuestions(this.startDate, this.endDate);
    }
  }


}
