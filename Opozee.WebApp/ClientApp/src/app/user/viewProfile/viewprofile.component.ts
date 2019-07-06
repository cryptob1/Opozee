import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { first } from 'rxjs/operators';
import { LocalStorageUser, NotificationsModel } from '../../_models';
import { UserProfileModel } from '../../_models/user';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';

@Component({
  templateUrl: 'viewProfile.component.html'

})
export class ViewProfileComponent implements OnInit {

  Id: number;
  _follow: string;
  userProfiledata: UserProfileModel
  //userProfiledata: {}
  localStorageUser: LocalStorageUser
  followersList: any
  followingList: any

  isRecordLoaded: boolean = false;
  notification: NotificationsModel[] = [];

  PagingModel = { 'UserId': 0, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, IsChecked: true, CheckedTab: "mybeliefs" }
  followingModel = { 'UserId': 0, 'Following': 0, IsFollowing: false }

  constructor(private route: ActivatedRoute, private userService: UserService, private router: Router, private toastr: ToastrService) {
    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];

      this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
      this.onchangeTab('mybeliefs')
      if (this.localStorageUser) {
        this.PagingModel.UserId = this.Id;
        this.getTabOneNotification(this.PagingModel)
      }
    }
    this._follow = "Follow";
  }

  ngOnInit() {
    if (localStorage.getItem('currentUser') == null) {
      this.logout() 
    }
    else {
      this.getUserProfile();
      this.PagingModel.UserId = this.Id;
    }
  }

  onFollowClick() {
    
    this.followingModel.UserId = this.localStorageUser.Id;
    this.followingModel.IsFollowing = true;
    this.followingModel.Following = this.Id;

    this.userService.postFollowing(this.followingModel).pipe(first())
      .subscribe(followings => {
        console.log(followings);
      },
      error => {
        if (error.status == 401) {
          this.logout();
          //this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
          //Observable.interval(1000)
          //  .subscribe((val) => {
          //    this.logout();
          //  });
        }
        else {
        
        }
      });
  }

  onchangeTab(data) {
    this.PagingModel.UserId = this.Id;
    this.PagingModel.CheckedTab = data;
    if (this.PagingModel.CheckedTab ==='mybeliefs') {
      this.getTabOneNotification(this.PagingModel)
    }
    else {
      this.getTabOneNotification(this.PagingModel)
    }   
  }

  getUserProfile() {
    //var Id = this.localStorageUser.Id;
    this.userService.getUserProfileWeb(this.Id).pipe(first())
      .subscribe(data => {
      this.userProfiledata = data
      //console.log(data);
      });
  }
    
  private getTabOneNotification(PagingModel) {
    var Id = this.Id;
    this.userService.getTabOneNotification(PagingModel).pipe(first()).subscribe(Notifications => {
      //console.log(Notifications);
      this.notification = Notifications;
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
    this.router.navigate(['/login']);
    //window.location.reload();
  }

  onFollowTab(tab) {

    this.PagingModel.UserId = this.Id;
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
  }

  viewProfile(id) {
    if (id === this.localStorageUser.Id) {
      this.router.navigate(['/profile/', id]);
    }
    else {
      this.router.navigateByUrl('/viewuser/' + id, { skipLocationChange: true }).then(() =>
        this.router.navigate(['/viewprofile/', id]));
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
      }, error => {
        if (error.status == 401) {
          this.logout();

          //this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
          //Observable.interval(1000)
          //  .subscribe((val) => {
          //    this.logout();
          //  });
        }
       
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
      }, error => {
        if (error.status == 401) {
          this.logout();

          //this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
          //Observable.interval(1000)
          //  .subscribe((val) => {
          //    this.logout();
          //  });
        }
       
      });
  }

}
