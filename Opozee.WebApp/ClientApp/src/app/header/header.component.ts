import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DataSharingService } from '../dataSharingService';
import { User, LocalStorageUser, PostQuestionDetail } from '../_models/user';
import { UserService } from '../_services';
import { QuestionListing } from '../_models/question';

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

  logout() {

    localStorage.removeItem('currentUser');
    window.location.reload();

  }

  ngOnInit() {

    debugger;
    this.dataSharingService.loginget.subscribe(data => {
      debugger;
      this.loginData = data
    })

    if (this.localStorageUser && this.localStorageUser.Id > 0) {
      debugger;
      this.isAuthenticateUserId = this.localStorageUser.Id;
      this.loginData = this.localStorageUser


    }

  }


  searchText(e) {
    debugger;
    if (e.keyCode == 13) {
      this.router.navigate(['/questionlisting/', this.searchTextModel]);
    }

  }

  searchTextButton(e) {
    debugger;
    this.router.navigate(['/questionlisting/', this.searchTextModel]);


  }
}
