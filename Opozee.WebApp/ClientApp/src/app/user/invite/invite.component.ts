import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalStorageUser } from '../../_models';

@Component({
  selector: 'app-invite',
  templateUrl: './invite.component.html',
  styleUrls: ['./invite.component.css']
})
export class InviteComponent implements OnInit {
  private readonly BASE_URL;

  referralCode: string;
  referralURL: string;
  localStorageUser: any;

  constructor(private route: ActivatedRoute, private router: Router, @Inject('BASE_URL') baseUrl: string) {
    debugger
    this.BASE_URL = baseUrl;
    this.referralURL = this.BASE_URL
    route.params.subscribe(params => {
      this.referralCode = params['code'];
      if (this.referralCode) {
        //redirect to register 
        this.router.navigate(['/register/' + this.referralCode])
      }
    })

  }

  ngOnInit() {

    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
    this.referralURL += 'invite' + '/' + this.localStorageUser.ReferralCode
  }

  copyInputMessage(inputElement) {
    inputElement.select();
    document.execCommand('copy');
    inputElement.setSelectionRange(0, 0);
  }

}
