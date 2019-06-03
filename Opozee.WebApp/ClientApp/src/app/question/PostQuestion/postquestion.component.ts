import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { User, LocalStorageUser } from '../../_models';
import { UserService, AlertService } from '../../_services';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { Router, ActivatedRoute } from '@angular/router';
import { debounce } from 'rxjs/operator/debounce';
import { ToastrService } from 'ngx-toastr';
import { ConfirmationDialogComponent } from '../../Shared/confirmationDialog/confirmationDialog.component';
import { MixpanelService } from '../../_services/mixpanel.service';

//import { ConfirmationDialogService } from '../../Shared/confirmationDialog/confirmationDialog.service';

@Component({
  templateUrl: 'postquestion.component.html',
  styleUrls: ['./postquestion.component.css']
})
export class PostQuestionComponent implements OnInit {

  @ViewChild('confirmationDialogComponent') confirmationDialogComponent: ConfirmationDialogComponent;
  @Output() event: EventEmitter<any> = new EventEmitter<any>();

  questionPostForm: FormGroup;
  loading = false;
  submitted = false;
  returnUrl: string;
  postQuestion: string;
  localStorageUser: LocalStorageUser;

  dropdownList = [];
  selectedItems = [];
  dropdownSettings = {};
  Usertags: string = '';

  editorConfig = {
    "editable": true,
    "spellcheck": true,
    "height": "100px",
    "minHeight": "100px",
    "width": "auto",
    "minWidth": "0",
    "translate": "yes",
    "enableToolbar": true,
    "showToolbar": false,
    "placeholder": "Enter question here...",
    "imageEndPoint": "",
    "toolbar": [
      ["bold", "italic", "underline","fontSize", "color"],
      ["cut", "copy", "delete" , "undo", "redo"],
      ["link", "unlink"]
    ]

  }


  users: User[] = [];
  constructor(private userService: UserService, private userservice: UserService, private alertService: AlertService, private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService, private mixpanelService: MixpanelService
    //private confirmationDialogService: ConfirmationDialogService
  ) {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));

    if (this.localStorageUser != null) {
      mixpanelService.init(this.localStorageUser['Email'])
    }

  }

  ngOnInit() {

    //this.loadAllUsers();
    this.questionPostForm = this.formBuilder.group({
      postQuestion: ['', [Validators.required, Validators.maxLength(299)]],
      hashtags: [''],
      ownerUserId: this.localStorageUser.Id 
    });


    this.GetAllTaggedDrop(); // Tag dropdown bind

    //test

    //this.selectedItems = [];
    this.dropdownSettings = {
      singleSelection: false,
      idField: 'UserID',
      textField: 'UserName',
      selectAllText: 'Select All',
      unSelectAllText: 'UnSelect All',
      itemsShowLimit: 3,
      allowSearchFilter: true
    }

  }

  // convenience getter for easy access to form fields
  get f() { return this.questionPostForm.controls; }

  //

  public openConfirmationDialog() {
    
    this.confirmationDialogComponent.show();
    //this.confirmationDialogService.confirm('Please confirm..', 'Do you really want to ... ?')
    //  .then((confirmed) => console.log('User confirmed:', confirmed))
    //  .catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
  }


  onSubmitNew() {
    this.submitted = true;
    debugger;
    // stop here if form is invalid
    if (this.questionPostForm.invalid) {
      this.toastr.error('', 'please Fill all Field !', { timeOut: 2000 });
      return;
    }
    let postQuestionTemp = this.questionPostForm.value.postQuestion.trim();
    let UsertagsTemp = this.Usertags.trim();
    this.questionPostForm.value.hashtags = this.questionPostForm.value.hashtags.trim().replace(/, /g, ',').replace(/ ,/g, ',').replace(/\s/g,',').replace(/#/g,'').toLowerCase();
    let taggedUserTemp = this.Usertags
    
    if (postQuestionTemp == '') {
      postQuestionTemp = 0;
      this.toastr.error('', 'please Fill all Field', { timeOut: 2000 });
      return
    }


    this.Usertags = this.Usertags.substring(0, this.Usertags.length - 1)
    this.questionPostForm.value.taggedUser = this.Usertags;
    this.loading = true;
    this.userservice.checkDuplicateQuestions(this.questionPostForm.value)
      .pipe(first())
      .subscribe(data => {
        if (data) {
          this.openConfirmationDialog();
          this.loading = false;
        }
        else {
          this.onSubmit();
        }
      },
        error => {
          this.alertService.error(error);
          this.loading = false;
        });
  }

  getPostQuestion() {
    this.onSubmit();
  }


  onSubmit() {
    this.submitted = true;
    debugger;
    // stop here if form is invalid
    if (this.questionPostForm.invalid) {
      this.toastr.error('', 'please Fill all Field !', { timeOut: 2000 });
      return;
    }
    let postQuestionTemp = this.questionPostForm.value.postQuestion.trim();
    let UsertagsTemp = this.Usertags.trim();
    this.questionPostForm.value.hashtags = this.questionPostForm.value.hashtags.trim();
    let taggedUserTemp = this.Usertags
       
    if (  postQuestionTemp == '') {
      postQuestionTemp = 0;
      this.toastr.error('', 'please Fill all Field', { timeOut: 2000 });
      return
    }


    this.Usertags = this.Usertags.substring(0, this.Usertags.length - 1)
    this.questionPostForm.value.taggedUser = this.Usertags;
    this.loading = true;
    this.userservice.postQuestionweb(this.questionPostForm.value)
      .pipe(first())
      .subscribe(data => {

        if (data) {
          if (data.Response.Question) {
            //console.log(data.Response.Question);
            this.alertService.success('Question Posted', true);
            this.mixpanelService.track('Posted Belief');
            this.router.navigate(['questiondetail/' + data.Response.Question]);

            //if (data.BalanceToken <= 0) {
            //  this.toastr.error('Token Blance 0', 'You have 0 tokens in your account. Please email us to refill the account to post opinion.', { timeOut: 5000 });
            //}
            //else {
            //  this.alertService.success('Question Posted', true);
            //  this.router.navigate(['']);
            //}
            this.loading = false;


          }

        }
        else {
          this.toastr.error('This question is already posted. Please enter a different question.');
          this.router.navigate(['']);
        }
        //}
        //else {
        //  this.toastr.error('This question is already posted. Please enter a diffrent question.');
        //  this.loading = false;
        //  return false;
        //}

      },
        error => {
          this.alertService.error(error);
          this.loading = false;
        });
  }

  onItemSelect(e) {
    debugger;
    this.Usertags += e.UserID + ','

  }

  onDeSelect(e) {
    debugger;
    var replacevalue = e.UserID + ','
    this.Usertags = this.Usertags.replace(replacevalue, '');

  }

  onSelectAll(e) {

    for (let i = 0; i < e.length; i++) {
      this.Usertags += e[i].UserID + ','
    }
  }


  GetAllTaggedDrop() {
    debugger;
    //var Id = this.localStorageUser.Id;
    this.userService.GetAllTaggedDropService().pipe(first()).subscribe(data => {
      debugger;
      this.dropdownList = data;

      //console.log(data);
    });
  }

}
