import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';
import { User, NotificationsModel, LocalStorageUser } from '../_models';
import { QuestionListing } from '../_models/question';
import { UserService } from '../_services';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';


@Component({
  templateUrl: 'notification.component.html',
  styleUrls: ['./notification.component.css']
})

export class NotificationComponent implements OnInit {
  currentUser: User;
  isRecordLoaded: boolean = false;
  
  notification: NotificationsModel[] = [];
  NotificationData: NotificationsModel[] = [];
  questionListing: QuestionListing[] = [];
  localStorageUser: LocalStorageUser;
  PagingModel = { 'UserId': 0, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, IsChecked: true }
  //IsChecked : any;
  // pager object
  pager: any = {};

  // paged items
  pagedItems: any[];

  constructor(private userService: UserService, private toastr: ToastrService) {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
  }

  ngOnInit() {
    
    this.setPageonpageLoad(1, 50)
    this.PagingModel.UserId = this.localStorageUser.Id;
    this.PagingPagesload(1, 10)
  }

  changeNotification(event) {
    debugger
    this.PagingModel.IsChecked = event.target.checked;
    if (this.PagingModel.IsChecked) {
      this.setPageonpageLoad(this.PagingModel.PageNumber, 50)
      this.PagingModel.UserId = this.localStorageUser.Id;
      this.PagingPagesload(this.PagingModel.PageNumber, 10)
    }
    else {
      this.setPageonpageLoad(this.PagingModel.PageNumber, 50)
      this.PagingModel.UserId = this.localStorageUser.Id;
      this.PagingPagesload(this.PagingModel.PageNumber, 10)
    }
  }

  private getAllNotification(PagingModel) {
    
    this.NotificationData = [];
    this.isRecordLoaded = false;
    var Id = this.localStorageUser.Id;
    this.userService.getAllNotification(PagingModel).pipe(first()).subscribe(Notifications => {
      this.NotificationData = [];
      if (Notifications) {
        if (Notifications.length > 0) {
          this.NotificationData = Notifications;
          this.PagingModel.TotalRecords = Notifications[0].TotalRecordcount
          
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
        this.logout();
        //this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
        //Observable.interval(1000)
        //  .subscribe((val) => {
        //    this.logout();
        //  });
      }
      this.isRecordLoaded = false;
    });
  }
  logout() {
    localStorage.removeItem('currentUser');
    window.location.reload();
  }

  PagingPagesload(PageNumber, PageSize) {
    this.PagingModel.PageNumber = PageNumber;
    this.PagingModel.PageSize = PageSize
    this.getAllNotification(this.PagingModel);
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


  html2text(text) {

    return String(text).replace(/<[^>]+>/gm, '').replace(/&amp;/g, "&").replace(/&nbsp;/g, ' ').replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/&lt;/g, "<").replace(/&gt;/g, ">");

       
  }


}
