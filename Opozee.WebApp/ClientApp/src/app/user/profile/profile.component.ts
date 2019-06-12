import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { first } from 'rxjs/operators';
import { LocalStorageUser, NotificationsModel } from '../../_models';
import { UserProfileModel } from '../../_models/user';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';


@Component({ templateUrl: 'profile.component.html' })
export class ProfileComponent implements OnInit {

  Id: number;
  userProfiledata: UserProfileModel
  //userProfiledata: {}
  localStorageUser: LocalStorageUser
  followersList: any
  followingList: any

  notification: NotificationsModel[] = [];
  profileData: any[] = [];

  //getUserIdbasedData: any;
  //getUserFollowerData: any;

  pager: any = {};
  // paged items
  pagedItems: any[];
  isRecordLoaded: boolean = false;

  PagingModel = { 'UserId': 0, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, IsChecked: true, CheckedTab: "mybeliefs" }
  //followingModel = { 'UserId': 0, 'Following': 0, IsFollowing: false }


  constructor(private route: ActivatedRoute, private userService: UserService, private toastr: ToastrService) {
    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];

      this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
      this.onchangeTab('mybeliefs')
      //this.getTabOneNotification(this.PagingModel)
      this.PagingModel.PageNumber = 1;
      this.PagingModel.TotalRecords = 5
    }
  }

  ngOnInit() {
    this.PagingModel.UserId = this.localStorageUser.Id;
    this.getUserProfile();
  }

  private getTabOneNotification(PagingModel) {
    this.profileData = [];
    var Id = this.localStorageUser.Id;
    this.userService.getTabOneNotification(PagingModel)
      .pipe(first()).toPromise()
      .then(data => {

        this.profileData = [];

        if (data) {
          if (data.length > 0) {
            this.profileData = data;

            try {
              this.profileData = this.profileData.map((x) => {
                if (x.QOCreationDate) {
                  let Valid10MinDate = new Date(x.QOCreationDate);
                  Valid10MinDate.setMinutes(Valid10MinDate.getMinutes() + 30);

                  let currentDate = new Date().toISOString().substring(0, new Date().toISOString().length - 1);
                  //console.log(Valid10MinDate, new Date(currentDate));
                  if (Valid10MinDate.getTime() > new Date(currentDate).getTime()) {
                    x.IsValidToDelete = true;
                  }
                }
                return x;
              });
            } catch (err) { }

            //console.log("profileData",this.profileData);
            this.PagingModel.TotalRecords = data[0].TotalRecordcount
          }
          else {
            this.PagingModel.TotalRecords = 0;
          }
          this.setPageonpageLoad(this.PagingModel.PageNumber, this.PagingModel.TotalRecords)
          this.isRecordLoaded = true
        }
        this.setPageonpageLoad(this.PagingModel.PageNumber, this.PagingModel.TotalRecords)
        this.isRecordLoaded = true

      }, error => {
        if (error.status == 401) {
          this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
          Observable.interval(1000)
            .subscribe((val) => {
              this.logout();
            });
        }
        this.isRecordLoaded = false;

      });

  }
  logout() {
    localStorage.removeItem('currentUser');
    window.location.reload();
  }
  onFollowTab(tab) {
    this.PagingModel.UserId = this.localStorageUser.Id;
    this.PagingModel.PageNumber = 1;
    this.PagingModel.PageSize = 10;
    this.followersList = [];
    this.followingList = [];

    if (tab === 1) {
      this.isRecordLoaded = true;
      this.userService.getMyFollowers(this.PagingModel)
        .pipe(first()).toPromise()
        .then(data => {
          this.followersList = data;
          console.log('followersList', data);
          this.isRecordLoaded = false;
        }, error => {
          if (error.status == 401) {
            this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
            Observable.interval(1000)
              .subscribe((val) => {
                this.logout();
              });
          }
          this.isRecordLoaded = false;

        });
    }
    else if (tab === 2) {
      this.isRecordLoaded = true;
      this.userService.getMyFollowing(this.PagingModel)
        .pipe(first()).toPromise()
        .then(data => {
          this.followingList = data;
          console.log('followingList', data);
          this.isRecordLoaded = false;
        }, error => {
          if (error.status == 401) {
            this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
            Observable.interval(1000)
              .subscribe((val) => {
                this.logout();
              });
          }
          this.isRecordLoaded = false;
        });
    }
  }

  onchangeTab(data) {
    
    this.PagingModel.UserId = this.localStorageUser.Id;
    this.PagingModel.CheckedTab = data;
    if (this.PagingModel.CheckedTab ==='mybeliefs') {
      this.getTabOneNotification(this.PagingModel)
      this.PagingModel.PageNumber = 1;
      this.PagingModel.TotalRecords = 5
    }
    else {
      this.getTabOneNotification(this.PagingModel)
      this.PagingModel.PageNumber = 1;
      this.PagingModel.TotalRecords = 5
    }
   
  }

  getUserProfile() {
    //var Id = this.localStorageUser.Id;
    this.userService.getUserProfileWeb(this.localStorageUser.Id).pipe(first()).subscribe(data => {
      this.userProfiledata = data
      //console.log(data);
    });
  }
  
  ///
  PagingPagesload(PageNumber, PageSize) {
    
   //his.questionGetModel.Search = this.search;
    this.PagingModel.PageNumber = PageNumber;
    this.PagingModel.PageSize = PageSize
    this.getTabOneNotification(this.PagingModel);

  }

  setPageonpageLoad(page, TotalRecords) {
    this.pager = this.getPager(TotalRecords, page);
  }

  setPage(page, TotalRecords) {    

    this.pager = this.getPager(this.PagingModel.TotalRecords, page);

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

  deleteQuestionBelief(tab, qData) {
        if (tab === 'myquestions') {
            var questionModel = { 'Id': qData.QuestionId, 'OwnerUserID': this.localStorageUser.Id }
            this.userService.deleteMyQuestion(questionModel)
                .pipe(first())
                .subscribe(data => {
                    if (data) {
                        this.profileData = this.profileData.filter(x => x.QuestionId != qData.QuestionId);
                        this.PagingModel.TotalRecords = this.profileData.length;
                        this.setPageonpageLoad(this.PagingModel.PageNumber, this.PagingModel.TotalRecords)
                        this.isRecordLoaded = true;
                        this.toastr.success('', 'Question deleted successfully.', { timeOut: 5000 });
                    }
                    else {
                        this.toastr.error('Wrong Record', 'Some error occured.', { timeOut: 5000 });
                    }
                },
                    error => {
                     
                      if (error.status == 401) {
                        this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
                        Observable.interval(1000)
                          .subscribe((val) => {
                            this.logout();
                          });
                      }
                      console.log('error: ', error)
                    });
        }
        else if (tab === 'mybeliefs') {
            var beliefModel = { 'Id': qData.OpinionId, 'OwnerUserID': this.localStorageUser.Id }
            this.userService.deleteMyBelief(beliefModel)
                .pipe(first())
                .subscribe(data => {
                    if (data) {
                        this.profileData = this.profileData.filter(x => x.OpinionId != qData.OpinionId);
                        this.PagingModel.TotalRecords = this.profileData.length;
                        this.setPageonpageLoad(this.PagingModel.PageNumber, this.PagingModel.TotalRecords)
                        this.isRecordLoaded = true;
                        this.toastr.success('', 'Belief deleted successfully.', { timeOut: 5000 });
                    }
                    else {
                        this.toastr.error('Wrong Record', 'Some error occured.', { timeOut: 5000 });
                    }
                },
                    error => {
                      
                      if (error.status == 401) {
                        this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
                        Observable.interval(1000)
                          .subscribe((val) => {
                            this.logout();
                          });
                      }
                      console.log('error: ', error)
                    });
        }
    }
  
  Unfollow(userId, tab) {

    let model = {
      UserId: this.localStorageUser.Id,
      IsFollowing: true,
      Following: userId
    }

    this.userService.unfollowUser(model).pipe(first())
      .subscribe(data => {
        this.getUserProfile();
        this.onFollowTab(tab);
    });
  }

  Follow(userId, tab) {
    let model = {
      UserId: this.localStorageUser.Id,
      IsFollowing: true,
      Following: userId
    }
    this.userService.followUser(model).pipe(first())
      .subscribe(data => {
        this.getUserProfile();
        this.onFollowTab(tab);
    });
  }

}
