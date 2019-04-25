import { Component, OnInit, ViewChild, EventEmitter } from '@angular/core';
import { Location } from "@angular/common";
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../_services/user.service';
import { first } from 'rxjs/operators';
import { LocalStorageUser, Comments } from '../_models/user';
import { PostQuestionDetail, BookMarkQuestionVM } from '../_models/user';
import { debounce } from 'rxjs/operator/debounce';
import { ToastrService } from 'ngx-toastr';
import { DialogPostBelief } from '../questionDetail/dialogPostBelief/dialogPostBelief.component';
import { HttpUrlEncodingCodec } from '@angular/common/http';

@Component({
  selector: 'questiondetail-component',
  templateUrl: './questiondetail.component.html',
  styleUrls: ['./questiondetail.component.css']
})

export class Questiondetail implements OnInit {

  @ViewChild('dialogPostBelief') dialogPostBelief: DialogPostBelief;

  model: any = {};
  postOpinionForm: FormGroup;
  loading = false;
  returnUrl: string;
  isAuthenticate: boolean;
  Id: number;
  Isclicked: boolean = false;
  comment: '';
  submitted: boolean = false;
  imageShowLike: number = -1;;
  imageShowDislike: number = -1;
  isWanttoSentComment: boolean = false;
  animal: string;
  name: string;
  countReactionScore: number;
  PostQuestionDetail: any;
  sharetext: string = '';//'Hey I’d like get your take on this question –';
  shareUrl: string;
  public _emitter: EventEmitter<any> = new EventEmitter();
  encoder: HttpUrlEncodingCodec = new HttpUrlEncodingCodec();

  dataModel = {
    'QuestId': 0, 'Comment': '',
    'CommentedUserId': 0,
    'Likes': 0,
    'OpinionAgreeStatus': 0,
    'Dislikes': 0,
    'CommentId': 0,
    'CreationDate': new Date(),
    'LikeOrDislke': false,
  }
  
  localStorageUser: LocalStorageUser;
  // PostQuestionDetailModel: { 'Comments': [], 'PostQuestionDetail':{}};
  PostQuestionDetailModel: BookMarkQuestionVM = new BookMarkQuestionVM();
 
  // isExpanded = false;

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private formBuilder: FormBuilder,
    private router: Router, private location: Location,
    private toastr: ToastrService
  ) {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));

    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];
    }
    this.shareUrl = window.location.origin + this.location.path();
   // this.PostQuestionDetailModel.PostQuestionDetailModel = new PostQuestionDetail();
  }
  
  //logout() { } 
  //toggle() {
  //  this.isExpanded = !this.isExpanded;
  //}

  ngOnInit() {
    //  {
    //  HashTags: "",
    //  IsBookmark: false,
    //  OwnerUserID: 0,
    //  OwnerUserName: "",
    //  Question: "",
    //  UserImage: "",
    //  YesCount: 0,
    //  NoCount: 0
    //}
    this.postOpinionForm = this.formBuilder.group({
      firstName: ['', Validators.required],
    });

    this.getQuestionDetail()
  }

  percentage: number = 0;

  getQuestionDetail() {
    
    this.userService.getquestionDetails(this.Id, this.localStorageUser.Id).subscribe(data => {
      
      this.PostQuestionDetailModel = data as BookMarkQuestionVM;
      this.PostQuestionDetailModel.comments = data['Comments'] as Comments[];
      
      let scoreYes = 0;
      let scoreNo = 0;

      this.PostQuestionDetailModel.comments.map((x) => {
        
        let _score = x.LikesCount - x.DislikesCount;
        if (x.IsAgree) {
          scoreYes = scoreYes + (_score > 0 ? _score : 0);      
        }
        else {
          scoreNo = scoreNo + (_score > 0 ? _score : 0); 
        }
       
      })

      this.percentage = (scoreYes / (scoreYes + scoreNo)) * 100;


      let _countLike = data['Comments'].map(c => c.LikesCount);
      var sumLike = _countLike.reduce(function (a, b) { return a + b; }, 0);

      let _countDislike = data['Comments'].map(c => c.DislikesCount);
      var sumDisLike = _countDislike.reduce(function (a, b) { return a + b; }, 0);

      let _count = data['Comments'].length;

      this.countReactionScore = sumLike + sumDisLike + _count;

      this.PostQuestionDetailModel.postQuestionDetail = data['PostQuestionDetail'] as PostQuestionDetail;


      this.shareUrl = "https://opozee.com/qid/" + (this.PostQuestionDetailModel.postQuestionDetail.Id);
      this.sharetext = this.PostQuestionDetailModel.postQuestionDetail.Question + " - See opposing views on Opozee.com!";

    });
  }

  saveLikeclick(Likes, index) {
    ;
    if (!Likes) { //hitting like
      this.imageShowLike = index;
      //this.imageShowDislike = -2;
      /////+
      this.dataModel.Likes = 1;
      this.dataModel.Dislikes = 0;
      this.dataModel.QuestId = this.Id;
      this.dataModel.Comment = this.comment;
      this.dataModel.CommentedUserId = this.localStorageUser.Id;
      this.dataModel.LikeOrDislke = true;
      //this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel);

    }
    else {
      this.imageShowLike = -1;

      this.dataModel.Likes = 0;
      this.dataModel.Dislikes = 0;
      this.dataModel.QuestId = this.Id;
      this.dataModel.Comment = this.comment;
      this.dataModel.CommentedUserId = this.localStorageUser.Id;
      this.dataModel.LikeOrDislke = true;
      //this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel);


    }
  }



  saveDislikeclick(DisLikes, index) {
    ;
    if (!DisLikes) { //clicking dislike
      this.imageShowDislike = index;
      //this.imageShowLike = -2;

      this.dataModel.Dislikes = 1;

      this.dataModel.Likes = 0;
      this.dataModel.QuestId = this.Id;
      this.dataModel.Comment = this.comment;
      this.dataModel.CommentedUserId = this.localStorageUser.Id;
      this.dataModel.OpinionAgreeStatus = 0;
      this.dataModel.LikeOrDislke = false;
      //this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel);
    }
    else {
      this.imageShowDislike = -1;

      this.dataModel.Dislikes = 0;
      this.dataModel.Likes = 0;
      this.dataModel.QuestId = this.Id;
      this.dataModel.Comment = this.comment;
      this.dataModel.CommentedUserId = this.localStorageUser.Id;
      this.dataModel.OpinionAgreeStatus = 0;
      this.dataModel.LikeOrDislke = false;
      //  this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel);


    }

  }

  clickYes() {
    this.dataModel.QuestId = this.Id;
    this.dataModel.Comment = this.comment;
    this.dataModel.CommentedUserId = this.localStorageUser.Id
    this.dataModel.OpinionAgreeStatus = 1
    this.Isclicked = true;

  }

  clickNO() {
    this.dataModel.QuestId = this.Id;
    this.dataModel.Comment = this.comment;
    this.dataModel.CommentedUserId = this.localStorageUser.Id
    this.dataModel.OpinionAgreeStatus = 0
    this.Isclicked = true;


  }

  cancelclick() {
    this.Isclicked = false;
    this.isWanttoSentComment = false;
  }

  saveOpinionclick() {

    ;
    this.submitted = true;
    ;
    if (this.dataModel.Comment == '' || this.dataModel.Comment == undefined) {
      return;
    }


    this.loading = true;
    this.userService.saveOpinionPost(this.dataModel)
      .pipe(first())
      .subscribe(data => {
        ;
        if (data.BalanceToken <= 0) {

          this.toastr.error('Token Blance 0', 'You have 0 tokens in your account. Please email us to refill the account to post opinion.', { timeOut: 5000 });
        }
        else {
          this.getQuestionDetail();
          this.toastr.success('Data save successfully', '');
        }
       
        this.loading = false;
        this.Isclicked = false;


      },
        error => {
          //this.alertService.error(error);
          //this.loading = false;
        });
  }

  searchForTag(hashtag) {
    this.router.navigateByUrl('/questionlistings/' + hashtag, { skipLocationChange: true }).then(() =>
      this.router.navigate(['/questions/', hashtag]));
    //this.router.navigate(['/questions'], { queryParams: { tag: 1 } });
  }

  SaveLikeDislike(dataModel) {
    ;
    this.loading = true;
    this.userService.SaveLikeDislikeService(this.dataModel)
      .pipe(first())
      .subscribe(data => {
        ;
        this.loading = false;
        this.getQuestionDetail();
        //this.toastr.success('Data save successfully', '');
        this.dataModel = {
          'QuestId': 0, 'Comment': '',
          'CommentedUserId': 0,
          'Likes': 0,
          'OpinionAgreeStatus': 0,
          'Dislikes': 0,
          'CommentId': 0,
          'CreationDate': new Date(),
          'LikeOrDislke': false,
        }
        //this.router.navigate(['/questiondetail/', this.Id]);

      },
        error => {
          //this.alertService.error(error);
          //this.loading = false;
        });

  }


  saveBookmarkQuestion(IsBookmark) {

    ;
    var dataBookMarkModel = {
      'QuestionId': this.Id,
      'IsBookmark': true,
      'UserId': this.localStorageUser.Id,
      'CreationDate': new Date(),
      'ModifiedDate': new Date(),
    }
    if (IsBookmark) {
      dataBookMarkModel.IsBookmark = false;
    }
    else {

      dataBookMarkModel.IsBookmark = true;
    }


    this.getQuestionDetail()
    this.loading = true;
    this.userService.saveBookmarkQuestionservice(dataBookMarkModel)
      .pipe(first())
      .subscribe(data => {
        ;
        if (data.BalanceToken <= 0) {
          this.toastr.error('Token Blance 0', 'You have 0 tokens in your account. Please email us to refill the account to post opinion.', { timeOut: 5000 });
        }
        else {
          this.getQuestionDetail();
          this.toastr.success('Data save successfully', '');
        }
 


      },
        error => {
          this.loading = false;
        });
    
  }
  
  showSuccess() {
    this.toastr.success('Hello world!', '');
  }

  openBeliefModal() {
    //this.isWanttoSentComment = true
    this.dataModel.QuestId = this.Id;
    this.dataModel.CommentedUserId = +this.localStorageUser.Id;

    console.log('data22', this.dataModel);
    this.dialogPostBelief.show(this.dataModel);
  }
}
