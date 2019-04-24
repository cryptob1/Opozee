import { Component, OnInit, Output, HostListener, ViewChild, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DataSharingService } from '../dataSharingService';
import { User, LocalStorageUser, PostQuestionDetail, UserProfileModel } from '../_models/user';
import { UserService } from '../_services';
import { ResetPassword } from '../user/resetPassword/resetPassword.component';
import { first } from 'rxjs/operators';


@Component({
  selector: 'header-component',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})

export class HeaderComponent implements OnInit {
  @ViewChild('resetPassword') resetPassword: ResetPassword;
  @Output() save: EventEmitter<any> = new EventEmitter<any>();

  model: any = {};
  loading = false;
  returnUrl: string;
  isAuthenticate: boolean;
  isAuthenticateUserId: number = 0;
  ImageURL: string = '';
  localStorageUser: LocalStorageUser;
  loginData: any;
  searchTextModel: string = '';
  userId: number;
  userProfiledata: UserProfileModel;
  // isExpanded = false;


  dataModel = {

    'UserLoggedId': 0,
    'EmailId': '',
  }


  constructor(private dataSharingService: DataSharingService, private route: ActivatedRoute, private router: Router, ) {

    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));

  }

  @HostListener('document:click', ['$event.target'])
  documentClick(target: any) {
    try {
      if (!target.className.endsWith("nav navbar-nav navbar-right")
        && !target.className.endsWith("ng-untouched ng-pristine ng-valid")
        && !target.className.endsWith("ng-valid ng-touched ng-dirty")        
        && !target.className.endsWith("ng-pristine ng-valid ng-touched")) {
        this.navbar();
      }
    } catch (err) {  }
  }

  logout() {
    localStorage.removeItem('currentUser');
    window.location.reload();
  }

  ngOnInit() {

    this.dataSharingService.loginget.subscribe(data => {
      this.loginData = data
    })

    if (this.localStorageUser && this.localStorageUser.Id > 0) {
      this.isAuthenticateUserId = this.localStorageUser.Id;
      this.loginData = this.localStorageUser
    }
   
  }

  searchText(e) {    
    if (e.keyCode == 13) {
      this.router.navigateByUrl('/questionlistings/'+ this.searchTextModel, { skipLocationChange: true }).then(() =>
        this.router.navigate(['/questionlisting/', this.searchTextModel]));
    }
  }

  searchTextButton(e) {
    this.router.navigateByUrl('/questionlistings/' + this.searchTextModel, { skipLocationChange: true }).then(() =>
      this.router.navigate(['/questionlisting/', this.searchTextModel]));
  }

  navbar() {
    document.getElementById('bs-example-navbar-collapse-1').classList.remove('show');
  }

  //resetPassword() {
  //  alert(1)
  //}

  openResetModal() {
    //debugger
    //this.isWanttoSentComment = true
    //this.dataModel.UserLoggedId = this.userId;
    //console.log('data22', this.dataModel);
    this.resetPassword.show();
  }
 
}
