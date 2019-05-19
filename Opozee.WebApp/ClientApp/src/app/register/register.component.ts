import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';

import { ToastrService } from 'ngx-toastr';

import { AlertService, UserService } from '../_services';

@Component({templateUrl: 'register.component.html'})
export class RegisterComponent implements OnInit {
    registerForm: FormGroup;
    loading = false;
  submitted = false;
  

    constructor(
        private formBuilder: FormBuilder,
        private router: Router,
        private userService: UserService,
      private alertService: AlertService,
    private toastr: ToastrService) {


    }


    ngOnInit() {
        this.registerForm = this.formBuilder.group({
            userName: ['', Validators.required],
            //lastName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6)]]
        });
    }

    // convenience getter for easy access to form fields
    get f() { return this.registerForm.controls; }

  onSubmit() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.registerForm.invalid) {
      return;
    }

    this.loading = true;
    this.userService.register(this.registerForm.value)
      .pipe(first())
      .subscribe(
        data => {
          this.alertService.success('Registration successful', true);
          
          if (data.Response) {
            //console.log(data.Response);
            if (data.Response.UserData) {
              let _user = data.Response.UserData;
              let contact = {
                'userName': _user.userName,
                'firstName': '',
                'lastName': '',
                'email': _user.Email
              }
              this.sendWelcomeMail(contact);

              this.toastr.success('Registration', 'Successful.', { timeOut: 5000 });

              this.router.navigate(['/login']);
            }
            else if (data.Response.Code=1) {
              this.toastr.error('Registration Failed', data.Response.Status, { timeOut: 8000 });
              this.router.navigate(['/login']);
            }
            
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
}
