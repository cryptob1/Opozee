import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../_services/user.service';
import { ToastrService } from 'ngx-toastr';
import { first } from 'rxjs/operators';

@Component({
  selector: 'reset-password',
  templateUrl: './resetPassword.component.html',
  styleUrls: ['./resetPassword.component.css']
})

export class ResetPassword implements OnInit {
  postBeliefForm: FormGroup;
  dataModel: any;

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


  @ViewChild('resetPassword') public resetPassword: ModalDirective;
  @Output() save: EventEmitter<any> = new EventEmitter<any>();


  constructor(private route: ActivatedRoute, private userService: UserService, private formBuilder: FormBuilder,
    private router: Router, private toastr: ToastrService) {
    this.dataModel = this.getModelSetting();
  }  

  ngOnInit() {
    this.editorConfigModal;
  }

 

  show(record?: any): void {
    debugger
   // alert(12);
    //this.dataModel.QuestId = question.QuestId;
    //this.dataModel.CommentedUserId = question.CommentedUserId;
    //this.dataModel.OpinionAgreeStatus = 0;

    //console.log('data', this.dataModel);
    this.resetPassword.show();
  }

  close() {
    this.resetPassword.hide();
  }

  submitForm() {
    console.log('data', this.dataModel);

    if (this.dataModel.Comment == '' || this.dataModel.Comment == undefined) {
      this.toastr.error('ERROR', 'Please enter belief.');
      return;
    }
    else if (this.dataModel.Comment.trim() == '') {
      this.toastr.error('ERROR', 'Please enter belief.');
      return;
    }
    else {
      this.userService.saveOpinionPost(this.dataModel)
        .pipe(first())
        .subscribe(data => {
          debugger;
          if (data.BalanceToken <= 0) {
            this.toastr.error('Token Blance 0', 'You have 0 tokens in your account. Please email us to refill the account to post opinion.', { timeOut: 5000 });
          }
          else {
            this.save.emit();
            this.toastr.success('Data save successfully', '');
            this.close();
          }

        },
          error => {
            this.toastr.error('Error', 'Something went wrong, please try again.');
            //this.alertService.error(error);
            //this.loading = false;
          });
    }
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
