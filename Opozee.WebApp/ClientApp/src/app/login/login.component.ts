import { Component, OnInit, EventEmitter, Output, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';

import { AlertService, AuthenticationService, UserService } from '../_services';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DataSharingService } from '../dataSharingService';
import { ToastrService } from 'ngx-toastr';
import { AuthService, FacebookLoginProvider, GoogleLoginProvider } from 'angular5-social-login';
//import { AuthService } from './auth/auth.service'; 
import * as AuthService1 from "auth0-js";
import { ForgotPassword } from '../user/forgotPassword/forgotPassword.component';
import { MixpanelService } from '../_services/mixpanel.service';
 

@Component({ templateUrl: 'login.component.html' })
export class LoginComponent implements OnInit {

  
  @ViewChild('forgotPassword') forgotPassword: ForgotPassword;

  Counter: number = 0;

  loginForm: FormGroup;
  loading = false;
  submitted = false;
  returnUrl: string ="";
  thirdPartyModel = { 'UserName': '', 'FirstName': '', 'LastName': '', 'DeviceType': 'Web', 'ThirdPartyId': '', 'Email': '', 'ThirdPartyType': 0 };

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authenticationService: AuthenticationService,
    private alertService: AlertService,
    private dataSharingService: DataSharingService,
    private toastr: ToastrService,
    private socialAuthService: AuthService,
    public auth: AuthService,
     private mixpanelService: MixpanelService
  ) {


  }

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      email: ['', Validators.required],
      password: ['', Validators.required]
    });

    // reset login status
    this.authenticationService.logout();

    // get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  // convenience getter for easy access to form fields
  get f() { return this.loginForm.controls; }


  loginAnon() {
    this.submitted = true;
    this.loading = true;

    this.loginForm.value.email = 'contactus@opozee.com'
    this.loginForm.value.password='useropz'

    this.authenticationService.login(this.loginForm.value)
      .pipe(first())
      .subscribe(data => {
        //debugger;
        if (data.Id != 0) {

          this.loading = false;
          this.toastr.success('Login', 'Ready to Opozee!', { timeOut: 1000 });
          this.dataSharingService.loginsetstate(data);
          // store user details and jwt token in local storage to keep user logged in between page refreshes
          localStorage.setItem('currentUser', JSON.stringify(data));
          //this.router.navigate(['']);
          this.mixpanelService.init('Anon');
          this.mixpanelService.track('Login');
          this.router.navigateByUrl(this.returnUrl);
        }

        else {
          this.loading = false;
          this.toastr.error('Invalid User', 'please check user name or Password !', { timeOut: 2000 });
        }
      },
        error => {
          this.alertService.error(error);
          this.loading = false;
          this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000 });
        });
  }

  onSubmit() {
    this.submitted = true;
    //debugger;
    // stop here if form is invalid
    if (this.loginForm.invalid) {
      return;
    }
    else {
      this.loginUser(this.loginForm.value);
    }
  }

  loginUser(_model) {

    this.loading = true;

    this.authenticationService.login(_model)
      .pipe(first())
      .subscribe(data => {
        //debugger;
        if (data.Id > 0) {

          this.loading = false;
          this.toastr.success('', 'Ready to Opozee!', { timeOut: 1000 });
          this.dataSharingService.loginsetstate(data);
          // store user details and jwt token in local storage to keep user logged in between page refreshes
          localStorage.setItem('currentUser', JSON.stringify(data));
          //this.router.navigate(['']);
          this.mixpanelService.init(_model.Email);
          this.mixpanelService.track('Login with email');

          this.router.navigateByUrl(this.returnUrl);
        }
        else if (data.Id == -1) {
          this.loading = false;
          this.toastr.error('', 'Please confirm your email address.', { timeOut: 3000 });
        }
        else {
          this.loading = false;
          this.toastr.error('Invalid User', 'please check user name or Password !', { timeOut: 3000 });
        }
      },
        error => {
          this.alertService.error(error);
          this.loading = false;
          this.toastr.error('Error Logging in', error.message + '', { timeOut: 3000 });
        });
  }
  
  socialSignIn(socialPlatform: string) {
    
    let socialPlatformProvider;
    if (socialPlatform == "facebook") {
      socialPlatformProvider = FacebookLoginProvider.PROVIDER_ID;

      this.socialAuthService.signIn(socialPlatformProvider).then(
        (userData) => {

          this.thirdPartyModel.ThirdPartyId = userData.id;
          this.thirdPartyModel.FirstName = userData.name.split(' ')[0];
          this.thirdPartyModel.LastName = userData.name.split(' ')[1];
          if (userData.email == null || userData.email == "") {
            this.thirdPartyModel.Email = userData.id+'@fb.com';
          } else {
            this.thirdPartyModel.Email = userData.email;
          }
          this.thirdPartyModel.ThirdPartyType = 0;

          this.authenticationService.loginWithFacebook(this.thirdPartyModel)
            .pipe(first())
            .subscribe(data => {
              if (data) {
                if (data.success) {
                  this.loading = false;
                  this.toastr.success('Login', 'Ready to Opozee!', { timeOut: 1000 });
                    this.dataSharingService.loginsetstate(data.data);
                  // store user details and jwt token in local storage to keep user logged in between page refreshes
                    localStorage.setItem('currentUser', JSON.stringify(data.data));
                  //this.router.navigate(['']);
                  this.mixpanelService.init(this.thirdPartyModel.Email);
                  this.mixpanelService.track('Login with facebook');
                  this.router.navigateByUrl(this.returnUrl);
                }
                else {
                  this.loading = false;
                  this.toastr.error('Invalid User', data.message, { timeOut: 2000 });
                }
              }
              else {
                this.loading = false;
                this.toastr.error('Invalid User', 'please check user name or Password !', { timeOut: 2000 });
              }
            },
              error => {
                this.alertService.error(error);
                this.loading = false;
                this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000 });
              })

          // Now sign-in with userData

        }
      );

    } else if (socialPlatform == "google") {

      socialPlatformProvider = GoogleLoginProvider.PROVIDER_ID;

      this.socialAuthService.signIn(socialPlatformProvider).then(
        (userData) => {

          this.thirdPartyModel.ThirdPartyId = userData.id;
          this.thirdPartyModel.FirstName = userData.name.split(' ')[0];
          this.thirdPartyModel.LastName = userData.name.split(' ')[1];
          this.thirdPartyModel.Email = userData.email;
          this.thirdPartyModel.ThirdPartyType = 2;
         
          this.authenticationService.loginWithGoogle(this.thirdPartyModel)
            .pipe(first())
            .subscribe(data => {
              
              if (data) {
                if (data.success) {
                  //console.log(data)
                  this.loading = false;
                  this.toastr.success('Login', 'Ready to Opozee!', { timeOut: 1000 });
                    this.dataSharingService.loginsetstate(data.data);
                  // store user details and jwt token in local storage to keep user logged in between page refreshes
                    localStorage.setItem('currentUser', JSON.stringify(data.data));

                  this.mixpanelService.init(this.thirdPartyModel.Email);
                  this.mixpanelService.track('Login with Google');
                  //this.router.navigate(['']);
                  this.router.navigateByUrl(this.returnUrl);
                }
                else {
                  this.loading = false;
                  this.toastr.error('Invalid User', data.message, { timeOut: 2000 });
                }
              }
              else {
                this.loading = false;
                this.toastr.error('Invalid User', 'please check user name or Password !', { timeOut: 2000 });
              }
            },
              error => {
                this.alertService.error(error);
                this.loading = false;
                this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000 });
              })

          // Now sign-in with userData

        }, error => {
          this.alertService.error(error);
          this.loading = false;
          this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000 });
        }
      )
    }

  }

  loginwithTwitteSecond(event) {
    
    var data = event.currentTarget.value.split('_')

    this.thirdPartyModel.ThirdPartyId = data[0];
    this.thirdPartyModel.Email = data[1];
    this.thirdPartyModel.FirstName = data[2].split(' ')[0];
    this.thirdPartyModel.LastName = data[2].split(' ')[1];
    this.thirdPartyModel.ThirdPartyType = 1;
    this.authenticationService.loginWithGoogle(this.thirdPartyModel)
      .pipe(first())
      .subscribe(data => {
      debugger;
        if (data) {
          if (data.success) {
            this.loading = false;
            this.toastr.success('Login', 'Ready to Opozee!', { timeOut: 1000 });
              this.dataSharingService.loginsetstate(data.data);
            // store user details and jwt token in local storage to keep user logged in between page refreshes
            localStorage.setItem('currentUser', JSON.stringify(data.data));
            //this.router.navigate(['']);
            this.router.navigateByUrl(this.returnUrl);
          }
          else {
            this.loading = false;
            this.toastr.error('Invalid User', data.message, { timeOut: 2000 });
          }
      }
      else {
        this.loading = false;
        this.toastr.error('Invalid User', 'please check user name or Password !', { timeOut: 2000 });
      }
    },
      error => {
        this.alertService.error(error);
        this.loading = false;
        this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000 });
      })


  }

  openforgotpasswordModel() {
     
    this.forgotPassword.show();
    this.mixpanelService.init('Anon');
    this.mixpanelService.track('Frogot password');

  }
}



