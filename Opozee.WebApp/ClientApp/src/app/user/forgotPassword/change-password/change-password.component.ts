import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalStorageUser } from '../../../_models';
import { AppConfigService } from '../../../appConfigService';
import { UserService, AuthenticationService } from '../../../_services';
import { first } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { DataSharingService } from '../../../dataSharingService';
import { MixpanelService } from '../../../_services/mixpanel.service';
import { changePasswordModel } from '../../../_models/reset.interface';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent implements OnInit {
  private readonly BASE_URL;

  code: string;
  userId: number;
  loading = false;
  returnUrl: string = "";
  public _changePasswordModel: changePasswordModel;
  dataModel: any;
  resetForm: FormGroup;
  submitted = false;
  passwordmatch: boolean = true;

  constructor(private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService,
    private userService: UserService,
    private authenticationService: AuthenticationService,
    private dataSharingService: DataSharingService,
    private mixpanelService: MixpanelService
  ) {

    this.route.queryParams.subscribe(params => {
      this.userId = +params['id'];
      this.code = params['code'];
    });
    //console.log(this.userId, this.code)
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  ngOnInit() {

    this.toastr.toastrConfig.timeOut = 1000000;
    this.toastr.toastrConfig.positionClass = 'toast-bottom-full-width';
    this.toastr.toastrConfig.preventDuplicates = true;
    this.toastr.toastrConfig.closeButton = true;

    this._changePasswordModel = {
      newpassword: '',
      confirmPassword: ''
    }

    this.loading = true;
    this.userService.ForgotPWDLinkVerify(this.userId, this.code)
      .pipe(first())
      .subscribe(
        data => {
          if (data.success) {
            this.toastr.info('', data.message, { timeOut: 5000000 });
            let _user = data.data;
            //console.log('emailVerification user', _user);
            //this.router.navigate(['/login']);
          }
          else {
            this.toastr.error('', data.message, { timeOut: 5000000 });
            this.router.navigate(['/login']);
          }
          this.loading = false;
        },
        error => {
          this.toastr.error('', 'Please enter a valid link.', { timeOut: 2000000 });
          this.router.navigate(['/login']);
          this.loading = false;
        });
  }

  changePWD(model: changePasswordModel, isValid: boolean) {
    debugger
    console.log(isValid);
    // call API to save customer
    console.log(model, isValid);
    this.loading = true;

    let _params = {
      Password: model.newpassword,
      UserId: this.userId
    }

    if (isValid) {
      this.authenticationService.changePassword(_params)
        .pipe(first())
        .subscribe(data => {
          if (data.success) {
            this.toastr.success('', data.message, { timeOut: 5000000 });
            let _user = data.data;
            //console.log('emailVerification user', _user);
            this.router.navigate(['/login']);
          }
          else {
            this.toastr.error('', data.message, { timeOut: 5000000 });
            this.router.navigate(['/login']);
          }
          this.loading = false;
        },
          error => {
            // this.alertService.error(error);
            this.loading = false;
            this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000000 });
          });
    }
  }



  close() {
    
  }
  
}
