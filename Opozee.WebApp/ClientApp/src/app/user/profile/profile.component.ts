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

  PagingModel = { 'UserId': 0, 'Search': '', 'PageNumber': 0, 'TotalRecords': 0, 'PageSize': 0, IsChecked: true, CheckedTab: "mybeliefs" }


  constructor(private route: ActivatedRoute, private userService: UserService ) {
    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];

      this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
      this.onchangeTab('mybeliefs')
      this.getTabOneNotification(this.PagingModel)
    }
  }

  onchangeTab(data) {
    debugger
    this.PagingModel.UserId = this.localStorageUser.Id;
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
    this.userService.getUserProfileWeb(this.localStorageUser.Id).pipe(first()).subscribe(data => {
      debugger;
      this.userProfiledata = data
      console.log(data);
    });
  }


  ngOnInit() {
    this.getUserProfile();
    this.PagingModel.UserId = this.localStorageUser.Id;
  }

  private getTabOneNotification(PagingModel) {
    debugger
    var Id = this.localStorageUser.Id;
    this.userService.getTabOneNotification(PagingModel).pipe(first()).subscribe(Notifications => {
     // debugger;
      console.log('Notifications',Notifications);
      this.notification = Notifications;
    });
  }
  



}
