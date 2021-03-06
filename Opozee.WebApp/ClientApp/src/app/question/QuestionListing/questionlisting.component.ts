import { Component, OnInit, OnDestroy } from '@angular/core';
import { Location } from "@angular/common";
import { first } from 'rxjs/operators';
import { ActivatedRoute, Router } from '@angular/router';
import { User, LocalStorageUser, PostQuestionDetail } from '../../_models';
import { UserService } from '../../_services';

import { Subscription } from 'rxjs';
import { ISubscription } from 'rxjs/Subscription';
import { MixpanelService } from '../../_services/mixpanel.service';
import { AppConfigService } from '../../appConfigService';
import { Meta } from '@angular/platform-browser'; 

@Component({
  selector: 'question-listing',
  templateUrl: 'questionlisting.component.html',
  styleUrls: ['questionlisting.component.css']

})
export class QuestionListingComponent implements OnInit, OnDestroy {
  private paramsSub: Subscription;
  currentUser: User;
  localStorageUser: LocalStorageUser;
  search: string ;
  hashTag: boolean =false;
  qid = -1;
  tabIndex = 0;

  popularhastags: any = [];

  //questionListing: QuestionListing[] = [];
  PostQuestionDetailList: PostQuestionDetail[] = [];
 

  isRecordLoaded: boolean = false;
  percentage: number = 0; 
  questionGetModel = { 'UserId': 0, 'isHashTag': false, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, 'qid': 0, 'Sort' :0 }

  private allItems: any[];
  showSlider: boolean = false;
  shareUrl: any;
  sharetext: any;
  //pager: any = {};
  // pagedItems: any[];
  sliderData: PostQuestionDetail[] = [];
  bountyQuestions: any;
  startDate: any;
  endDate: any;

  constructor(private userService: UserService, private route: ActivatedRoute, private router: Router,
    private location: Location, private mixpanelService: MixpanelService, private configService: AppConfigService, private meta: Meta) {

    this.startDate = this.configService.bountyStartDate;
    this.endDate = this.configService.bountyEndDate;

    this.showSlider = this.location.path() ? false : true;

    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
    //this.hashTag = false;

    if (this.localStorageUser != null) {
      mixpanelService.init(this.localStorageUser['Email'])
    }


 

    if (this.route.snapshot.params["search"]) {
      this.search = this.route.snapshot.params["search"];
    }
    else if (this.route.snapshot.params["tag"]) {
      this.hashTag = true;
      this.search = this.route.snapshot.params["tag"];

    }
    else if (this.route.snapshot.params["qid"]) {
      this.qid = this.route.snapshot.params["qid"];
    }
    else {
  //default to dailyfive
      //this.questionGetModel.isHashTag = true;
      //this.hashTag = true;

      //this.search = 'DailyFive';
      //this.questionGetModel.Search = this.search;
    }

    this.paramsSub = route.params.subscribe(params => {
      this.initialize();
    });

    this.questionGetModel.PageNumber = 1;//+localStorage.getItem('PageNumber');

    this.questionGetModel.Sort = +localStorage.getItem('Sort');
  }

  dropdownSort(kind: number) {
    //console.log(kind);
    this.isRecordLoaded = false;
    this.questionGetModel.PageNumber = 1;//localStorage.getItem('PageNumber') ? +localStorage.getItem('PageNumber') : 1;
    this.questionGetModel.Sort = kind;
    localStorage.setItem('Sort', kind.toString());
    this.getAllQuestionlist(this.questionGetModel);
  }

  ngOnInit() {
    localStorage.removeItem('hasRedirectBack');
    this.location.subscribe(x => {
      localStorage.setItem('hasRedirectBack', "Yes");
    });
    if (!localStorage.getItem('hasRedirectBack'))
      localStorage.removeItem('PageNumber');

    this.shareUrl = "https://opozee.com/qid/";
    this.sharetext = " - See opposing views at ";

    if (this.qid>0 && this.localStorageUser != null) {
      
      this.goToqDetail();
    }


  }

  ngOnDestroy() {
    this.paramsSub.unsubscribe();
    this.location.subscribe(x => x).unsubscribe();
  }

  initialize() {
    
    this.getHastagsRecords();
    this.questionGetModel.isHashTag = this.hashTag;
    this.questionGetModel.qid = this.qid;


    this.questionGetModel.Search = this.search;
    if (this.localStorageUser) {
      this.questionGetModel.UserId = this.localStorageUser.Id
    }
    else {
      this.questionGetModel.UserId = 0;
    }
 

    this.questionGetModel.PageNumber = 1; //localStorage.getItem('PageNumber') ? +localStorage.getItem('PageNumber') : 1;

    //this.questionGetModel.PageNumber = 1;
    this.questionGetModel.PageSize = 10;
    this.questionGetModel.TotalRecords = 5;
    this.questionGetModel.Sort = +localStorage.getItem('Sort');

    let savedht = localStorage.getItem('savedtab');
    let savedtabindex = localStorage.getItem('savedtabindex');
      
    //console.log(savedht);
    if (savedht == undefined || savedht == "" || savedht == null || savedht == "All") {
      if (savedtabindex != ""){
        this.tabIndex = +savedtabindex;
      }
      this.getAllQuestionlist(this.questionGetModel);

    }
    else {
      //console.log("switch tab");
      this.switchTab(savedht, +localStorage.getItem('savedtabindex'));
    }
    //this.getBountyQuestionsByDates();

  }
   

  // getting popular Hastags
  private getHastagsRecords() {


    this.userService.getPopularHasTags().pipe(first()).subscribe(data => {

      this.popularhastags = data.reduce((arr, _item) => {
        let exists = !!arr.find(x => x.HashTag === _item.HashTag);
        if (!exists) {
          arr.push(_item);
        }

        return arr;
      }, []);

      //this.popularhastags = this.popularhastags.slice(0, 1);

       
      //this.popularhastags.unshift({ 'HashTag': 'All' });


      this.popularhastags.push({ 'HashTag': 'All' });
       
      //console.log(this.popularhastags);

    });
  }


  private getBountyQuestionsByDates() {
    
    this.bountyQuestions = [];
    this.isRecordLoaded = false;
    this.userService.getBountyQuestions(this.startDate, this.endDate)
      .subscribe(data => {
        this.bountyQuestions = data;
        this.PercentageCalc(this.bountyQuestions); 
        this.isRecordLoaded = true;
      },
        error => {
          this.isRecordLoaded = true;
          console.log('err', error);
        });
  }

  private getAllQuestionlist(questionGetModel) {
    console.log(questionGetModel);
    this.userService.getAllQuestionlist(questionGetModel).subscribe(data => {

      if (data) {
        if (data.length > 0) {
          
          this.PostQuestionDetailList = this.responseData(data, questionGetModel.PageSize);
          this.questionGetModel.TotalRecords = data[0].TotalRecordcount

          if (this.qid != -1) {
            this.PostQuestionDetailList[0].comments = data[0]['Comments'];


            this.meta.addTag({ name: 'description', content: data[0]['Question'] });

            this.meta.updateTag({ content: data[0]['Question'] }, 'name="description"');

            //this.meta.updateTag({ name: 'description', content: data[0]['Question']});


            // <!-- Facebook meta data -->
            this.meta.addTags([
              { property: 'og:title', content: data[0]['Question']  },
              
            ]);
          }
          //console.log('page=', questionGetModel.PageNumber, 'records=', this.PostQuestionDetailList.length)

          ////----Slider
          //this.sliderData = [];

          //this.PostQuestionDetailList.map((x) => {
          //  console.log(x.Question);
          //  console.log(x.IsSlider);
          //  if (x.IsSlider) {
          //    this.sliderData.push(x);
          //  }

          //})

          //this.PostQuestionDetailList.map((x) => {
          //  if (x.YesCount > 0 && x.NoCount > 0) {
          //    this.sliderData.push(x);
          //  }
          //  else if (x.YesCount > 0 || x.NoCount > 0) {
          //    this.sliderData.push(x);
          //  }
          //})

          //this.sliderData.sort(function (a, b) {
          //  if (a.MostYesLiked === null) return 1;
          //  else if (b.MostYesLiked === null) return -1;
          //  else {
          //    return a.MostYesLiked < b.MostYesLiked ? -1 : a.MostYesLiked > b.MostYesLiked ? 1 : 0;
          //  }
          //});

          //this.sliderData.sort(function (a, b) {
          //  if (a.MostNoLiked === null) return 1;
          //  else if (b.MostNoLiked === null) return -1;
          //  else {
          //    return a.MostNoLiked < b.MostNoLiked ? -1 : a.MostNoLiked > b.MostNoLiked ? 1 : 0;
          //  }
          //});
          //----------------------------------
          this.PercentageCalc(this.PostQuestionDetailList);
          //this.setPageonpageLoad(this.questionGetModel.PageNumber, this.questionGetModel.TotalRecords)
          this.isRecordLoaded = true
        } else {
          //this.setPageonpageLoad(this.questionGetModel.PageNumber, this.questionGetModel.TotalRecords)
          this.isRecordLoaded = true
        }

      }
    }, error => {
      this.isRecordLoaded = false;
    });
    this.isRecordLoaded = false;
  }

  private PercentageCalc(data) {
    try {
      let scoreYes = 0;
      let scoreNo = 0;
      let _score = 0;
      data.map((x) => {

        if (x.Comments) {

          x.Comments.map(y => {

            _score = y.LikesCount - y.DislikesCount;
            if (y.IsAgree) {
              scoreYes = scoreYes + (_score > 0 ? _score : 0);
            }
            else {
              scoreNo = scoreNo + (_score > 0 ? _score : 0);
            }
          });
        }

        x.percentage = +((scoreYes / (scoreYes + scoreNo)) * 100);
        if (isNaN(x.percentage))
          x.percentage = 0;
        scoreYes = 0;
        scoreNo = 0;
      })
    }
    catch (err) { }
  }
   
   

  goToqDetail() {
    
    this.router.navigate(['/questiondetail/', this.qid]);
  }

  searchForTag(hashtag) {
    this.router.navigateByUrl('/questionlistings/' + hashtag, { skipLocationChange: true }).then(() =>
      this.router.navigate(['/questions/', hashtag]));
    //this.router.navigate(['/questions'], { queryParams: { tag: 1 } });
  }


  switchTab(ht, index) {

    localStorage.setItem('savedtab', ht);
    localStorage.setItem('savedtabindex', index);
    

    this.isRecordLoaded = false;


    this.questionGetModel.PageNumber = 1;//localStorage.getItem('PageNumber') ? +localStorage.getItem('PageNumber') : 1;
    this.questionGetModel.PageSize = 10;
    this.questionGetModel.TotalRecords = 5;

    if (ht == 'All') {
      this.questionGetModel.isHashTag = false;
      this.hashTag = false;
      this.search = null;
      this.questionGetModel.Search = this.search;
    } else {
      this.questionGetModel.isHashTag = true;
      this.hashTag = true;
 
      this.search = ht;
      this.questionGetModel.Search = this.search;
    }
    this.tabIndex = index;
    //console.log(this.questionGetModel);
    this.getAllQuestionlist(this.questionGetModel);
  }
   
 
  onScroll(event) {

    //console.log("ssss");
    //console.log(event);
    //console.log(window.pageYOffset);
    this.questionGetModel.PageNumber += 1;
    this.questionGetModel.PageSize = 10;
    localStorage.setItem('PageNumber', ""+this.questionGetModel.PageNumber );

    //debugger
    this.userService.getAllQuestionlist(this.questionGetModel).subscribe(data => {

      if (data) {
        if (data.length > 0) {

          for (var i = 0; i < data.length; i++) {
            this.PostQuestionDetailList.push(data[i]);
          }

          this.questionGetModel.TotalRecords = data[0].TotalRecordcount

          if (this.qid != -1) {
            this.PostQuestionDetailList[0].comments = data[0]['Comments'];
          }
          //----------------------------------
          this.PercentageCalc(this.PostQuestionDetailList);

          //console.log('onScroll page=', this.questionGetModel.PageNumber, 'records=', this.PostQuestionDetailList.length)

          // this.setPageonpageLoad(this.questionGetModel.PageNumber, this.questionGetModel.TotalRecords)
          //this.isRecordLoaded = true
        } else {
          // this.setPageonpageLoad(this.questionGetModel.PageNumber, this.questionGetModel.TotalRecords)
          //this.isRecordLoaded = true
        }

      }
    }, error => {
    });

  }

  responseData(data: any, pageSize: number): any {
    try {
      if (data) return data.slice(0, pageSize)
      else return data;
    }
    catch (err) {
      return data;
    }
  }

}
