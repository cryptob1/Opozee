import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalStorageUser } from '../../_models';
import { AppConfigService } from '../../appConfigService';
import { UserService, AuthenticationService } from '../../_services';
import { first } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { DataSharingService } from '../../dataSharingService';
import { MixpanelService } from '../../_services/mixpanel.service';

@Component({
  selector: 'app-email-verification',
  templateUrl: './email-verification.component.html',
  styleUrls: ['./email-verification.component.css']
})
export class EmailVerificationComponent implements OnInit {
  private readonly BASE_URL;

  code: string;
  userId: number;
  loading = false;
  returnUrl: string = "";

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

    this.userService.emailVerification(this.userId, this.code)
      .pipe(first())
      .subscribe(
        data => {
          if (data.success) {
            this.toastr.success('', data.message, { timeOut: 500000 });
            let _user = data.data;
            //console.log('emailVerification user', _user);
            this.loginUser({ email: _user.Email, password: _user.Password, IsVerificationLogin: true});
            //this.router.navigate(['/login']);
          }
          else {
            this.toastr.error('', data.message, { timeOut: 5000000 });
            this.router.navigate(['/login']);
          }
        },
        error => {
          //this.toastr.error('', 'Something went wrong. Please try again.', { timeOut: 5000 });
          this.router.navigate(['/login']);
        });
  }


  loginUser(_model) {

    this.loading = true;

    this.authenticationService.login(_model)
      .pipe(first())
      .subscribe(data => {

        if (data.Id > 0) {
          this.loading = false;
          this.toastr.success('', 'Login successfully!', { timeOut: 2000 });
          this.dataSharingService.loginsetstate(data);
          // store user details and jwt token in local storage to keep user logged in between page refreshes
          localStorage.setItem('currentUser', JSON.stringify(data));
          this.mixpanelService.init(_model.Email);
          this.mixpanelService.track('Login with email');

          this.router.navigateByUrl(this.returnUrl);
        }
        else if (data.Id == -1) {
          this.loading = false;
          this.toastr.error('', 'Please confirm your email address.', { timeOut: 3000 });
          this.router.navigate(['/login']);
        }
        else {
          this.loading = false;
          this.toastr.error('Invalid User', 'please check user name or Password !', { timeOut: 3000 });
          this.router.navigate(['/login']);
        }
      },
        error => {
          this.loading = false;
          this.toastr.error('Error Logging in', error.message + '', { timeOut: 3000 });
          this.router.navigate(['/login']);
        });
  }


}
