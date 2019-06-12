import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../../_services/user.service';
import { ActivatedRoute, Router } from '@angular/router';
import { UserEditProfileModel } from '../../_models/user';
import { HttpClient, HttpRequest, HttpEventType, HttpResponse } from '@angular/common/http'
import { LocalStorageUser } from '../../_models';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';


@Component({ templateUrl: 'editProfile.component.html' })
export class EditProfileComponent implements OnInit {
  edtiProfileForm: FormGroup;
  loading: boolean = false;
  submitted = false;
  userId: number = 0;
  userEditProfileModel: UserEditProfileModel;
  public progress: number;
  public message: string;
  imageUrl: string = "";
  fileToUpload: File = null;
  localStorageUser: any;
  isSocialLogin: boolean = false;

  constructor(private userService: UserService, private formBuilder: FormBuilder, private route: ActivatedRoute, private toastr: ToastrService) {
    if (this.route.snapshot.params["Id"]) {
      this.userId = this.route.snapshot.params["Id"];

      this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
      this.isSocialLogin = this.localStorageUser.IsSocialLogin;
    }
  }

  ngOnInit() {
    this.edtiProfileForm = this.formBuilder.group({
      userName: ['', Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      userId: [''],
      IsSocialLogin: [this.isSocialLogin]
    });
        
    this.getUserProfile();
    this.formControlOnSocialLogin(this.isSocialLogin);
  }

  formControlOnSocialLogin(isSocial) {
    if (isSocial) {
      const emailControl = this.edtiProfileForm.get('email');
      emailControl.clearValidators();
      emailControl.updateValueAndValidity();
      const passwordControl = this.edtiProfileForm.get('password');
      passwordControl.clearValidators();
      passwordControl.updateValueAndValidity();
    }    
  }

  // convenience getter for easy access to form fields
  get f() { return this.edtiProfileForm.controls; }

  getUserProfile() {
    //var Id = this.localStorageUser.Id;
    this.userService.getEditUserProfileWeb(this.localStorageUser.Id)
      .pipe(first())
      .subscribe(data => {
        //console.log('getUserProfile: ', data);
        //this.userEditProfileModel = data
        //this.edtiProfileForm.setValue(data);
        this.edtiProfileForm.controls['userName'].setValue(data.UserName);
        this.edtiProfileForm.controls['firstName'].setValue(data.FirstName);
        this.edtiProfileForm.controls['lastName'].setValue(data.LastName);
        this.edtiProfileForm.controls['email'].setValue(data.Email);
        this.edtiProfileForm.controls['password'].setValue(data.Password);
        this.edtiProfileForm.controls['userId'].setValue(data.UserId);
        this.imageUrl = data.ImageURL;
        this.isSocialLogin = data.IsSocialLogin;
        this.formControlOnSocialLogin(data.IsSocialLogin);
      }, error => {
        console.log('error: ', error);
        this.loading = false;
      });
  }


  onSubmit() {
    
    this.submitted = true;
    if (this.edtiProfileForm.invalid) {
      return;
    }

    this.loading = true;
    this.userService.editUserprofile(this.edtiProfileForm.value)
      .pipe(first())
      .subscribe(data => {
        this.loading = false;
        if (data) {
          //console.log('editUserprofile: ', data);
          if (data.success) {
            this.toastr.success('', 'Change successful!', { timeOut: 1000 });
          } else {
            this.toastr.error('', data.message + '', { timeOut: 2000 });
          }
        }
      },
        error => {
          //this.alertService.error(error);
          if (error.status == 401) {
            this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
            Observable.interval(1000)
              .subscribe((val) => {
                this.logout();
              });
          }
          this.loading = false;
        });
  }

  logout() {
    localStorage.removeItem('currentUser');
    window.location.reload();
  }
  handleFileInput(file: FileList) {
    this.fileToUpload = file.item(0);

    //Show image preview
    var reader = new FileReader();
    reader.onload = (event: any) => {
      this.imageUrl = event.target.result;
    }
    reader.readAsDataURL(this.fileToUpload);
  }



  UploadFile() {
    debugger;
    this.userService.uploadfileService(this.userId , this.fileToUpload).subscribe(
      data => {
        
        //Caption.value = null;
        //Image.value = null;
        this.getUserProfile();
        //this.imageUrl = "../../../assets/images/user.png";
        this.toastr.success('Image', 'Change successful!', { timeOut: 1000 });
      },
      error => {
        if (error.status == 401) {
          this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
          Observable.interval(1000)
            .subscribe((val) => {
              this.logout();
            });
        }
        else {
          this.toastr.error('Failed to upload image -', error.message + '', { timeOut: 2000 });
        }
       
      }
    );
  }


}
