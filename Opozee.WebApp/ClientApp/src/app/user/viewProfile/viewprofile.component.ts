import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { first } from 'rxjs/operators';
import { LocalStorageUser, NotificationsModel } from '../../_models';
import { UserProfileModel } from '../../_models/user';


@Component({
  templateUrl: 'viewProfile.component.html'

})
export class ViewProfileComponent implements OnInit {

  Id: number;
  Follow: string;
  userProfiledata: UserProfileModel
  //userProfiledata: {}
  localStorageUser: LocalStorageUser

  notification: NotificationsModel[] = [];

  PagingModel = { 'UserId': 0, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, IsChecked: true, CheckedTab: "mybeliefs" }
  followingModel = { 'UserId': 0, 'Following': 0, IsFollowing: false }

  constructor(private route: ActivatedRoute, private userService: UserService ) {
    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];

      this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
      this.onchangeTab('mybeliefs')
      this.getTabOneNotification(this.PagingModel)
    }
    this.Follow = "Follow";
  }


  onFollowClick() {
    debugger
    this.followingModel.UserId = this.localStorageUser.Id;
    this.followingModel.IsFollowing = true;
    this.followingModel.Following = this.Id;

    this.userService.postFollowing(this.followingModel).pipe(first()).subscribe(followings => {
      // debugger;
      console.log(followings);
    });

  }


  onchangeTab(data) {
    debugger
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
    this.userService.getUserProfileWeb(this.Id).pipe(first()).subscribe(data => {
      debugger;
      this.userProfiledata = data
      //console.log(data);
    });
  }


  ngOnInit() {
    this.getUserProfile();
    this.PagingModel.UserId = this.Id;
  }

  private getTabOneNotification(PagingModel) {
    debugger
    var Id = this.Id;
    this.userService.getTabOneNotification(PagingModel).pipe(first()).subscribe(Notifications => {
     // debugger;
      console.log(Notifications);
      this.notification = Notifications;
    });
  }
  



}
