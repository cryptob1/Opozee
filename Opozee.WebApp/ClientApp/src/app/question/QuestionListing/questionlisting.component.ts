import { Component, OnInit, OnDestroy } from '@angular/core';
import { Location } from "@angular/common";
import { first } from 'rxjs/operators';
import { ActivatedRoute, Router } from '@angular/router';
import { User, LocalStorageUser, PostQuestionDetail } from '../../_models';
import { UserService } from '../../_services';

import { Subscription } from 'rxjs';
import { ISubscription } from 'rxjs/Subscription';



@Component({
  selector: 'question-listing',
  templateUrl: 'questionlisting.component.html',
  styleUrls: ['questionlisting.component.css']

})
export class QuestionListingComponent implements OnInit, OnDestroy {
  private paramsSub: Subscription;
  currentUser: User;
  localStorageUser: LocalStorageUser;
  search: string;
  hashTag: boolean = false;
  qid = -1;
  //questionListing: QuestionListing[] = [];
  PostQuestionDetailList: PostQuestionDetail[] = [];


  isRecordLoaded: boolean = false;
  percentage: number = 0;
  questionGetModel = { 'UserId': 0, 'isHashTag': false, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, 'qid': 0 }

  private allItems: any[];

  shareUrl: any;
  sharetext: any;
  // pager object
  pager: any = {};
  // paged items
  pagedItems: any[];
  sliderData: any[] = [];
  constructor(private userService: UserService, private route: ActivatedRoute, private router: Router,
    private location: Location) {

    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
    this.hashTag = false;
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

    this.paramsSub = route.params.subscribe(params => {
      this.initialize();
    });
    this.questionGetModel.PageNumber = +localStorage.getItem('PageNumber');

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

  }

  ngOnDestroy() {
    this.paramsSub.unsubscribe();
    this.location.subscribe(x => x).unsubscribe();
  }

  initialize() {
    this.questionGetModel.isHashTag = this.hashTag;
    this.questionGetModel.qid = this.qid;


    this.questionGetModel.Search = this.search;
    if (this.localStorageUser) {
      this.questionGetModel.UserId = this.localStorageUser.Id
    }
    else {
      this.questionGetModel.UserId = 0;
    }
    //this.questionGetModel.PageNumber = 1;

    this.questionGetModel.PageNumber = localStorage.getItem('PageNumber') ? +localStorage.getItem('PageNumber') : 1;

    this.questionGetModel.TotalRecords = 5
    this.getAllQuestionlist(this.questionGetModel);
    this.getAllSliderQuestionlist(this.questionGetModel);

  }


  private getAllSliderQuestionlist(questionGetModel) {

    this.userService.getAllSliderQuestionlist(questionGetModel).subscribe(data => {

      if (data) {
        if (data.length > 0) {


          //----Slider
          this.sliderData = [];

          data.map((x) => {
            console.log(x.Question);
            console.log(x.IsSlider);
            if (x.IsSlider) {
              this.sliderData.push(x);
            }

          })



        } else {

        }

      }
    }, error => {


    });

  }

  private getAllQuestionlist(questionGetModel) {

    this.userService.getAllQuestionlist(questionGetModel).subscribe(data => {

      if (data) {
        if (data.length > 0) {
          this.PostQuestionDetailList = data;
          this.questionGetModel.TotalRecords = data[0].TotalRecordcount

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
          this.PercentageCalc(data);
          this.setPageonpageLoad(this.questionGetModel.PageNumber, this.questionGetModel.TotalRecords)
          this.isRecordLoaded = true
        } else {
          this.setPageonpageLoad(this.questionGetModel.PageNumber, this.questionGetModel.TotalRecords)
          this.isRecordLoaded = true
        }

      }
    }, error => {
      this.isRecordLoaded = false;

    });
    this.isRecordLoaded = false;
  }

  private PercentageCalc(data) {
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

  private getAllQuestionlistPaging(questionGetModel) {

    this.userService.getAllQuestionlist(questionGetModel).subscribe(data => {

      if (data) {
        if (data.length > 0) {
          this.PostQuestionDetailList = data;
          this.questionGetModel.TotalRecords = data[0].TotalRecordcount;
          this.PercentageCalc(data);
        }
        this.isRecordLoaded = true
      }
      this.isRecordLoaded = true

    }, error => {
      this.isRecordLoaded = false;
    });
  }

  searchForTag(hashtag) {
    this.router.navigateByUrl('/questionlistings/' + hashtag, { skipLocationChange: true }).then(() =>
      this.router.navigate(['/questions/', hashtag]));
    //this.router.navigate(['/questions'], { queryParams: { tag: 1 } });
  }

  PagingPagesload(PageNumber, PageSize) {

    this.questionGetModel.Search = this.search;
    this.questionGetModel.PageNumber = PageNumber;
    this.questionGetModel.PageSize = PageSize
    this.getAllQuestionlistPaging(this.questionGetModel);
    localStorage.setItem('PageNumber', PageNumber);
  }

  setPageonpageLoad(page, TotalRecords) {
    this.pager = this.getPager(TotalRecords, page);
  }

  setPage(page, TotalRecords) {
    localStorage.setItem('PageNumber', page);
    this.pager = this.getPager(this.questionGetModel.TotalRecords, page);

    if (this.pager.totalPages >= page) {
      this.PagingPagesload(this.pager.currentPage, this.pager.pageSize);
    }
  }

  getPager(totalItems: number, currentPage: number = 1, pageSize: number = 10) {
    // calculate total pages

    let totalPages = Math.ceil(totalItems / pageSize);

    // ensure current page isn't out of range
    if (currentPage < 1) {
      currentPage = 1;
    } else if (currentPage > totalPages) {
      currentPage = totalPages;
    }

    let startPage: number, endPage: number;
    if (totalPages <= 10) {
      // less than 10 total pages so show all
      startPage = 1;
      endPage = totalPages;
    } else {
      // more than 10 total pages so calculate start and end pages
      if (currentPage <= 6) {
        startPage = 1;
        endPage = 10;
      } else if (currentPage + 4 >= totalPages) {
        startPage = totalPages - 9;
        endPage = totalPages;
      } else {
        startPage = currentPage - 5;
        endPage = currentPage + 4;
      }
    }

    // calculate start and end item indexes
    let startIndex = (currentPage - 1) * pageSize;
    let endIndex = Math.min(startIndex + pageSize - 1, totalItems - 1);

    // create an array of pages to ng-repeat in the pager control
    let pages = Array.from(Array((endPage + 1) - startPage).keys()).map(i => startPage + i);

    // return object with all pager properties required by the view
    return {
      totalItems: totalItems,
      currentPage: currentPage,
      pageSize: pageSize,
      totalPages: totalPages,
      startPage: startPage,
      endPage: endPage,
      startIndex: startIndex,
      endIndex: endIndex,
      pages: pages
    };
  }



}
