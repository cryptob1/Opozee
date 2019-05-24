import { Component, OnInit, ViewChild, EventEmitter } from '@angular/core';
import { Location } from "@angular/common";
import { ActivatedRoute, Router, RouterModule } from '@angular/router'; 
import { first } from 'rxjs/operators';
import { debounce } from 'rxjs/operator/debounce';
import { UserService } from '../../_services';
 

@Component({
  selector: 'app-bounty-questions',
  templateUrl: './bounty-questions.component.html',
  styleUrls: ['./bounty-questions.component.css']
})
export class BountyQuestionsComponent implements OnInit {

  bountyQuestion: any;
  loading: boolean = false;
  constructor(private route: ActivatedRoute, private userService: UserService)
  {
        
  }

  ngOnInit() {

    this.getBountyQuestions();
  }
  
  getBountyQuestions(startDate?, endDate?) {
    this.loading = true;
    this.userService.getBountyQuestions(startDate, endDate)
      .subscribe(data => {
        this.bountyQuestion = data;
        this.loading = false;
      },
        error => {
          this.loading = false;
          console.log('GetBountyQuestions err', error);
        });
  }


}
