import { Component, OnInit, Output, HostListener, ViewChild, EventEmitter, OnDestroy, Inject, Renderer2 } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DataSharingService } from '../dataSharingService';
import { User, LocalStorageUser, PostQuestionDetail, UserProfileModel } from '../_models/user';
import { UserService } from '../_services';
import { ResetPassword } from '../user/resetPassword/resetPassword.component';
import { first } from 'rxjs/operators';
import 'rxjs/add/observable/interval';
import { Observable } from 'rxjs/Observable';
import { DOCUMENT } from '@angular/platform-browser';
import { Toast } from 'ngx-toastr';

@Component({
  selector: 'header-component',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})

export class HeaderComponent implements OnInit, OnDestroy {
  @ViewChild('resetPassword') resetPassword: ResetPassword;
  @Output() save: EventEmitter<any> = new EventEmitter<any>();

  showPopup: boolean = false;
  showPopup2: boolean = false;
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
    showNotificationIcon: boolean = false;
  numOfNotifications: number = 0;

  dataModel = {

    'UserLoggedId': 0,
    'EmailId': '',
  }


  constructor(private dataSharingService: DataSharingService, private route: ActivatedRoute, private router: Router,
    private userService: UserService, @Inject(DOCUMENT) private document: Document, private renderer: Renderer2) {

      this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
    this.showPopup = true;
    this.showPopup2 = false;
    //if (this.localStorageUser != null) {//
    if (localStorage.getItem('popupShown') ==null)  {
      this.showPopup = true;
      localStorage.setItem('popupShown', '1')
    }
    else if (JSON.parse(localStorage.getItem('popupShown')) == '1') {
      this.showPopup2 = true;
      this.showPopup = false;

      localStorage.setItem('popupShown', '2')
    }
    else {
      this.showPopup = false;
      this.showPopup2 = false;

    }
 
  }

  @HostListener('document:click', ['$event.target'])
  documentClick(target: any) {
    try {
      if (!target.className.endsWith("nav navbar-nav navbar-right")
        && !target.className.endsWith("ng-untouched ng-pristine ng-valid")
        && !target.className.endsWith("ng-valid ng-touched ng-dirty")        
        && !target.className.endsWith("ng-pristine ng-valid ng-touched")
        && !target.className.endsWith("badge bg-red")
        && !target.className.endsWith("cls-profile")) {
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
            this.loginData = this.localStorageUser;

             Observable.interval(100000)
                 .subscribe((val) => {  this.checkNotification() });

            this.checkNotification()
      }
  }

  ngOnDestroy(): void {
    this.renderer.removeClass(this.document.body, 'popup-backdrop')
  }

  checkNotification() {
    this.userService.checkNotification(this.localStorageUser.Id)
      .pipe(first())
      .subscribe(data => {
        //alert('checkNotification');
        if (data) {
          if (data.notification) {
            this.showNotificationIcon = data.notification.length > 0 ? true : false;
            this.numOfNotifications = data.notification.length;
          }
          else this.showNotificationIcon = false;
        }
        else this.showNotificationIcon = false;
      },
        error => {
          console.log('error: ', error)
          this.showNotificationIcon = false;
        });
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
    document.getElementById('bs-example-navbar-collapse-1').classList.remove('in');
    this.checkNotification();
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

  hidePopup() {
    this.showPopup = false;
    this.renderer.removeClass(this.document.body, 'popup-backdrop')
  }

  popup() {
    this.showPopup = true;
    window.scroll(0, 0);
    this.renderer.addClass(this.document.body, 'popup-backdrop')
    //document.getElementById('popup-container').classList.add('show'); 
  }


  hidePopup2() {
    this.showPopup2 = false;
    this.renderer.removeClass(this.document.body, 'popup-backdrop')
  }

  popup2() {
    this.showPopup2 = true;
    window.scroll(0, 0);
    this.renderer.addClass(this.document.body, 'popup-backdrop')
    //document.getElementById('popup-container').classList.add('show'); 
  }
}
