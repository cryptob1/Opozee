import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LocalStorageUser } from '../../_models';
import { AppConfigService } from '../../appConfigService';
import { UserService } from '../../_services';
import { first } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-email-verification',
  templateUrl: './email-verification.component.html',
  styleUrls: ['./email-verification.component.css']
})
export class EmailVerificationComponent implements OnInit {
  private readonly BASE_URL;

  code: string;
  userId: number;

  constructor(private route: ActivatedRoute, private router: Router, private toastr: ToastrService,
    private userService: UserService) {

    this.route.queryParams.subscribe(params => {
      this.userId = +params['id'];
      this.code = params['code'];
    });
    console.log(this.userId, this.code)
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
            this.toastr.success('', data.message, { timeOut: 5000000 });
            this.router.navigate(['/login']);
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


}
