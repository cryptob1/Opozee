import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';

import { ToastrService } from 'ngx-toastr';

import { AlertService, UserService } from '../_services';
import { MixpanelService } from '../_services/mixpanel.service';

@Component({ templateUrl: 'register.component.html' })
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  loading = false;
  submitted = false;
  referral: string;
  isValidCode: boolean = false;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private userService: UserService,
    private alertService: AlertService,
    private toastr: ToastrService,
    private mixpanelService: MixpanelService) {

    route.params.subscribe(params => {
      this.referral = params['code'];
    })

  }


  ngOnInit() {

    this.registerForm = this.formBuilder.group({
      userName: ['', Validators.required],
      //lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      referralCode: [this.referral]
    });
    //this.registerForm.referral=this.referral;
  }

  // convenience getter for easy access to form fields
  get f() { return this.registerForm.controls; }

  onSubmit() {
    
    this.submitted = true;
    let _referralCode = this.registerForm.get('referralCode').value;
    // stop here if form is invalid
    if (this.registerForm.invalid) {
      return;
    }
    else {
      _referralCode = _referralCode ? _referralCode.trim() : _referralCode;
      if (_referralCode) {
        this.userService.checkReferralCode(_referralCode)
          .pipe(first())
          .toPromise()
          .then(
            data => {
              if (data) {
                this.registerUser(this.registerForm.value);
              }
              else {
                this.toastr.error('', 'Please enter a valid referral code.', { timeOut: 4000 });
              }
            },
            error => {
              this.registerUser(this.registerForm.value);
            });

      }
      else {
        this.registerUser(this.registerForm.value);
      }
    }
  }

  private registerUser(registerForm) {
    this.loading = true;
    this.userService.register(registerForm)
      .pipe(first())
      .subscribe(
        data => {
          if (data) {
            if (data.success) {
              this.toastr.success('', data.message, { timeOut: 8000 });
              let _user = data.data;
              let contact = {
                'userName': _user.UserName,
                'firstName': '',
                'lastName': '',
                'email': _user.Email
              }
              //this.sendWelcomeMail(contact);
              //this.toastr.success('Registration', 'Successful.', { timeOut: 5000 });
              this.mixpanelService.init(_user.Email);
              this.mixpanelService.track('Signedup');
              this.router.navigate(['/login']);
              this.loading = false;
            }
            else {
              this.toastr.error('Registration Failed', data.message, { timeOut: 8000 });
              this.loading = false;
            }
          }
          else {
            this.toastr.error('', 'Registration Failed', { timeOut: 8000 });
            this.loading = false;
          }
        },
        error => {
          this.alertService.error(error);
          this.loading = false;
        });
  }

  private sendWelcomeMail(contact) {
    this.userService.sendWelcomeMail(contact)
      .pipe(first())
      .subscribe(data => { });
  }

  checkCodeValidation(referralCode) {
    let code = referralCode ? referralCode.trim() : referralCode;
    if (code) {
      this.checkIfValidCode(code);
    }
  }

  checkIfValidCode(referralCode): number {
    let code = referralCode ? referralCode.trim() : referralCode;
    if (code) {
      this.userService.checkReferralCode(code)
        .pipe(first())
        .toPromise()
        .then(
          data => {
            if (data) {
              this.isValidCode = true;
              return 1;
            }
            else {
              this.isValidCode = false;
              return 0;
            }
          },
          error => {
            console.log('checkCodeValidation', error);
            return -1;
          });
    }
    return -1;
  }

}
