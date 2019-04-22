import { Component, OnInit, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DataSharingService } from '../dataSharingService';
import { User, LocalStorageUser, PostQuestionDetail } from '../_models/user';
import { UserService } from '../_services';
 

@Component({
  selector: 'header-component',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})

export class HeaderComponent implements OnInit {
  model: any = {};
  loading = false;
  returnUrl: string;
  isAuthenticate: boolean;
  isAuthenticateUserId: number = 0;
  ImageURL: string = '';
  localStorageUser: LocalStorageUser;
  loginData: any;
  searchTextModel: string = '';
  // isExpanded = false;
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
}
