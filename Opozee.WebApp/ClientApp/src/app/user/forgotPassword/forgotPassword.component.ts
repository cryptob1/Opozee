import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { ToastrService } from 'ngx-toastr';
import { first } from 'rxjs/operators';
import { AuthenticationService, AlertService } from '../../_services';

@Component({
  selector: 'forgot-password',
  templateUrl: './forgotPassword.component.html',
  styleUrls: ['./forgotPassword.component.css']
})

export class ForgotPassword implements OnInit {
  

  forgotForm: FormGroup;
  loading = false;
  submitted = false;

  editorConfigModal = {
    "editable": true,
    "spellcheck": true,
    "height": "100px",
    "minHeight": "100px",
    "width": "auto",
    "minWidth": "0",
    "translate": "yes",
    "enableToolbar": true,
    "showToolbar": true,
    "placeholder": "Share your belief..",
    "imageEndPoint": "",
    "toolbar": [
      ["bold", "italic", "underline", "fontSize", "color"],
      ["cut", "copy", "delete", "undo", "redo"],
      ["link", "unlink"]
    ]

  }


  @ViewChild('forgotPassword', { static: true }) public forgotPassword: ModalDirective;

  @Output() save: EventEmitter<any> = new EventEmitter<any>();


  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private formBuilder: FormBuilder,
    private router: Router,
    private toastr: ToastrService,
    private authenticationService: AuthenticationService,
    private alertService: AlertService,
  ) {
   
  }  

  ngOnInit() {
    this.forgotForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

 
  // convenience getter for easy access to form fields
  get f() { return this.forgotForm.controls; }



  show(): void {

    this.forgotPassword.show();
  }

  close() {
    this.forgotPassword.hide();
  }

  onSubmit() {
    console.log('on submit');
    this.submitted = true;
    // stop here if form is invalid
    if (this.forgotForm.invalid) {
      return;
    }
    this.loading = true;
    this.authenticationService.forgotPassword(this.forgotForm.value)
      .pipe(first())
      .subscribe(data => {
        //console.log(data);
      },
        error => {
          this.alertService.error(error);
          this.loading = false;
          this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000 });
        });
  }

}
