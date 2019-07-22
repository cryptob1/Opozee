import { Component, OnInit, ViewChild, Output, EventEmitter, Inject } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { ToastrService } from 'ngx-toastr';
import { first, retry } from 'rxjs/operators';
import { MixpanelService } from '../../_services/mixpanel.service';
import { DOCUMENT } from '@angular/platform-browser';
import { Observable } from 'rxjs';

@Component({
  selector: 'dialog-post-belief',
  templateUrl: './dialogPostBelief.component.html',
  styleUrls: ['./../questiondetail.component.css']
})

export class DialogPostBelief implements OnInit {
  postBeliefForm: FormGroup;
  dataModel: any;
  loading: boolean = false;
  url: string;

  //formdata;
  editorConfigModal = {
    "editable": true,
    "spellcheck": true,
    "height": "100px",
    "minHeight": "100px",
    "width": "auto",
    "minWidth": "0",
    "translate": "yes",
    "enableToolbar": true,
    "showToolbar": false,
    "placeholder": "Share your belief in 400 characters or less..",
    "imageEndPoint": "",
    "toolbar": [
      ["bold", "italic", "underline", "fontSize", "color"],
      ["cut", "copy", "delete", "undo", "redo"],
      ["link", "unlink"]
    ]

  }


  @ViewChild('dialogPostBelief') public dialogPostBelief: ModalDirective;
  @Output() save: EventEmitter<any> = new EventEmitter<any>();


  constructor(private route: ActivatedRoute, private userService: UserService, private formBuilder: FormBuilder,
    private router: Router, private toastr: ToastrService, private mixpanelService: MixpanelService, @Inject(DOCUMENT) private document: any)  {
    this.dataModel = this.getModelSetting();
    this.dataModel.OpinionAgreeStatus = 0;  
  }  

  ngOnInit() {
        
    this.editorConfigModal;
    this.postBeliefForm = this.formBuilder.group({
      Comment: ['', [Validators.required, Validators.maxLength(400)]],
      LongForm: [''],
      OpinionAgreeStatus: [this.dataModel.OpinionAgreeStatus],
      QuestId: [this.dataModel.QuestId],
      CommentedUserId: [this.dataModel.CommentedUserId]

    });
   
  }

  // convenience getter for easy access to form fields
  get f() { return this.postBeliefForm.controls; }

  public keyUp(event: any) {
    let text = this.postBeliefForm.get('Comment').value;
    
    if (text!=null) {
      if (text.length >= 400) {


        if (event.key != 'Backspace') {
          event.preventDefault();
          event.stopPropagation();
        }
      }
    }

  
  }


  public onPaste(e) {
    e.preventDefault();
    e.stopPropagation();
    let texto = e.clipboardData.getData('text/plain');
    this.document.execCommand('insertText', false, texto);
    return false;
  }
 
  show(question?: any): void {
    this.postBeliefForm.reset();
    this.dataModel.QuestId = question.QuestId;
    this.dataModel.CommentedUserId = question.CommentedUserId;
    this.dataModel.OpinionAgreeStatus = 0;
 
    this.dialogPostBelief.show();
  }

  close() {
    this.dataModel.Comment = '';
    this.dataModel.LongForm = '';
    this.dialogPostBelief.hide();
  }

  submitForm() {
    
    this.postBeliefForm.patchValue({
      OpinionAgreeStatus: this.dataModel.OpinionAgreeStatus,
      QuestId: this.dataModel.QuestId,
      CommentedUserId: this.dataModel.CommentedUserId
   });
    
    let getComment = this.postBeliefForm.get('Comment').value;
    if (getComment == '' || getComment == undefined) {
      this.toastr.error('ERROR', 'Please enter belief.');
      return;
    }
    else if(getComment.trim() == '') {
      this.toastr.error('ERROR', 'Please enter belief.');
      return;
    }
    else {
      this.loading = true;
      this.userService.CheckDuplicateBelief(this.postBeliefForm.value)
        .pipe(first())
        .subscribe(data => {
          if (data) {
            this.toastr.error('', 'You already have posted this belief.', { timeOut: 3000 });
            this.loading = false;
            return;
          }
          else {
            this.saveOpinionPost(this.postBeliefForm.value);
            this.mixpanelService.track('Posted Belief');
          }

        },
          error => {
            this.saveOpinionPost(this.postBeliefForm.value);
            this.mixpanelService.track('Posted Belief');
          });
    }
  }


  saveOpinionPost(model) {
    
    this.loading = true;
    //console.log(model);
    this.userService.saveOpinionPost(model)
      .pipe(first())
      .subscribe(data => {
        
        this.loading = false;
        if (data.BalanceToken == -2) {
          this.toastr.error('', 'Please confirm your email address.', { timeOut: 5000 });
        }
        else if (data.BalanceToken <= 0) {
          this.toastr.error('Token Blance 0', 'You have 0 tokens in your account. Please email us to refill the account to post opinion.', { timeOut: 5000 });
        }
        else {
          this.save.emit();
          this.toastr.success('Posted!', '');
          this.close();
          this.dataModel.Commment = '';

          this.mixpanelService.track('Posted Belief');
        }

      },
        error => {
          
          if (error.status == 401) {
            this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
            Observable.interval(1000)
              .subscribe((val) => {
                this.logout();
              });
          }
          this.toastr.error('Error', 'Something went wrong, please try again.');
          this.loading = false;
          //this.alertService.error(error);
        });
  }

  
  onSelectFile(event) { // called each time file input changes
      if (event.target.files && event.target.files[0]) {
        var reader = new FileReader();

        reader.readAsDataURL(event.target.files[0]); // read file as data url

        reader.onload = (event) => { // called once readAsDataURL is completed
          
          this.url = event.target.result;
          console.log(this.url);
        }
      }
  }
   
  logout() {
    localStorage.removeItem('currentUser');
    window.location.reload();
  }

  setOpinionAgreeStatus(status: number) {
    this.dataModel.OpinionAgreeStatus = status;
  }

  getModelSetting() {
    return {
      'QuestId': 0,
      'Comment': '',
      'CommentedUserId': 0,
      'Likes': 0,
      'OpinionAgreeStatus': 0,
      'Dislikes': 0,
      'CommentId': 0,
      'CreationDate': new Date(),
      'LikeOrDislke': false,
    }
  }

}
