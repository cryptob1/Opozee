import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { first } from 'rxjs/operators';
import { LocalStorageUser, NotificationsModel } from '../../_models';
import { UserProfileModel } from '../../_models/user';


@Component({ templateUrl: 'profile.component.html' })
export class ProfileComponent implements OnInit {

  Id: number;
  userProfiledata: UserProfileModel
  //userProfiledata: {}
  localStorageUser: LocalStorageUser

  notification: NotificationsModel[] = [];
  profileData: NotificationsModel[] = [];

  pager: any = {};
  // paged items
  pagedItems: any[];
  isRecordLoaded: boolean = false;

  PagingModel = { 'UserId': 0, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, IsChecked: true, CheckedTab: "mybeliefs" }


  constructor(private route: ActivatedRoute, private userService: UserService ) {
    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];
      
      this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
      this.onchangeTab('mybeliefs')
      //this.getTabOneNotification(this.PagingModel)
      this.PagingModel.PageNumber = 1;
      this.PagingModel.TotalRecords = 5
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
      ;
      this.userProfiledata = data
      console.log(data);
    });
  }
  
  ngOnInit() {
    this.getUserProfile();
    this.PagingModel.UserId = this.localStorageUser.Id;
  }

  private getTabOneNotification(PagingModel) {
    this.profileData = [];
    var Id = this.localStorageUser.Id;
    this.userService.getTabOneNotification(PagingModel).pipe(first()).subscribe(data => {
    
      this.profileData = [];
      
      if (data) {
        if (data.length > 0) {
          this.profileData = data;
          //console.log("dt",this.profileData);
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
      this.isRecordLoaded = false;

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




}
