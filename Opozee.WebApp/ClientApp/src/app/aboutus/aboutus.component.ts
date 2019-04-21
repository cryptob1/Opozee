import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { UserService } from '../_services/user.service';
import { ToastrService } from 'ngx-toastr';
import { first } from 'rxjs/operators';

@Component({
  selector: 'app-aboutus',
  templateUrl: './aboutus.component.html',
  styleUrls: ['./aboutus.component.css']
})
export class aboutusComponent implements OnInit {
  contactForm: FormGroup;
  contact: any;
  loading: boolean= false;

  constructor(private route: ActivatedRoute, private userService: UserService,
    private router: Router, private alertService: ToastrService) {
    this.contact = this.getModelSetting();
  }

  ngOnInit() {
    this.router.events.subscribe((evt) => {
      if (!(evt instanceof NavigationEnd)) {
        return;
      }
      window.scrollTo(0, 0)
    });
  }

  submitForm() {

    this.loading = true;
    this.userService.sendContactMail(this.contact)
      .pipe(first())
      .subscribe(data => {

        this.alertService.success('Mail has been sent successfully.', '');
        //this.router.navigate(['/questionlisting']);
        this.contact = this.getModelSetting();
        this.loading = false;

      },
        error => {
          this.alertService.error("Please enter a valid email address.");
          this.loading = false;
        });
  }

  getModelSetting() {
    return {
      'firstName': '',
      'lastName': '',
      'email': '',
      'phone': '',
      'message': ''
    }
  }

}
