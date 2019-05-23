import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LocalStorageUser } from '../../_models';
import { AppConfigService } from '../../appConfigService';

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
  coinPerReferral: number;
  totalReferred: number;

  constructor(private route: ActivatedRoute, private router: Router, private config: AppConfigService,
    @Inject('BASE_URL') baseUrl: string) {

    this.BASE_URL = baseUrl;
    this.coinPerReferral = this.config.coinPerReferral;
    //this.referralURL = this.BASE_URL
    route.params.subscribe(params => {
      let _referralCode = params['code'];
      if (_referralCode) {
        //redirect to register 
        this.router.navigate(['/register/' + _referralCode])
      }
    })
  }

  ngOnInit() {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
    this.referralCode = this.localStorageUser.ReferralCode;
    this.referralURL = this.BASE_URL + 'invite' + '/' + this.referralCode;
    this.totalReferred = this.localStorageUser.TotalReferred ? this.localStorageUser.TotalReferred : 0;
  }

  copyInputMessage(inputElement) {
    inputElement.select();
    document.execCommand('copy');
    inputElement.setSelectionRange(0, 0);
  }

  copyLink(referralLink) {
    let selBox = document.createElement('textarea');
    selBox.style.position = 'fixed';
    selBox.style.left = '0';
    selBox.style.top = '0';
    selBox.style.opacity = '0';
    selBox.value = referralLink;
    document.body.appendChild(selBox);
    selBox.focus();
    selBox.select();
    document.execCommand('copy');
    document.body.removeChild(selBox);
  }

}
