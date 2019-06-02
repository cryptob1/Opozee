import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { ToastrService } from 'ngx-toastr';
import { first } from 'rxjs/operators';
import { AbstractControl } from '@angular/forms';
import { AuthenticationService, AlertService } from '../../_services';


import { FormsModule } from '@angular/forms';
import { resetModel } from '../../_models/reset.interface';



@Component({
  selector: 'reset-password',
  templateUrl: './resetPassword.component.html',
  styleUrls: ['./resetPassword.component.css']
})

export class ResetPassword implements OnInit {
  public resetModel: resetModel;
  dataModel: any;
  resetForm: FormGroup;
  loading = false;
  submitted = false;
  passwordmatch :boolean = true;
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


  @ViewChild('resetPassword', { static: true }) public resetPassword: ModalDirective;
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
   
    this.resetModel = {
      oldpassword:'',
      newpassword: '',
      confirmPassword: ''
    }
  }

  get f() { return this.resetForm.controls; }

  close() {
    this.resetPassword.hide();
  }

  

  reset(model: resetModel , isValid: boolean) {
    console.log(isValid);
    // call API to save customer
    console.log(model, isValid);
    this.loading = true;
    this.authenticationService.resetPassword(model)
      .pipe(first())
      .subscribe(data => {
       // console.log(data);
      },
        error => {
          this.alertService.error(error);
          this.loading = false;
          this.toastr.error('Error Logging in', error.message + '', { timeOut: 2000 });
        });
  }

  
 

  show(record?: any): void {
    
    this.resetPassword.show();
  }


}
