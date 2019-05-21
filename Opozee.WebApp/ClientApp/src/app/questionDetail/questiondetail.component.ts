import { Component, OnInit, ViewChild, EventEmitter } from '@angular/core';
import { Location } from "@angular/common";
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../_services/user.service';
import { first } from 'rxjs/operators';
import { LocalStorageUser, Comments } from '../_models/user';
import { PostQuestionDetail, BookMarkQuestionVM } from '../_models/user';
import { debounce } from 'rxjs/operator/debounce';
import { ToastrService } from 'ngx-toastr';
import { DialogPostBelief } from '../questionDetail/dialogPostBelief/dialogPostBelief.component';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { MixpanelService } from '../_services/mixpanel.service';

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

  sliderData: PostQuestionDetail[] = [];



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
    private toastr: ToastrService,
    private mixpanelService: MixpanelService
  ) {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));

    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];
    }
    this.shareUrl = window.location.origin + this.location.path();
    this.router.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };
   // this.PostQuestionDetailModel.PostQuestionDetailModel = new PostQuestionDetail();
  }
  
  //logout() { } 
  //toggle() {
  //  this.isExpanded = !this.isExpanded;
  //}

  html2text(text) {

    return String(text).replace(/<[^>]+>/gm, '').replace(/&amp;/g, "&").replace(/&nbsp;/g, ' ').replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/&lt;/g, "<").replace(/&gt;/g, ">");


  
  
  }


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

    this.route.params.subscribe(
      params => {
        
        this.getQuestionDetail();
      }
    );


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
      this.sharetext = this.html2text(this.PostQuestionDetailModel.postQuestionDetail.Question) + " - See opposing views at ";


      console.log(this.PostQuestionDetailModel);
      this.getAllSliderQuestionlist(this.Id, this.PostQuestionDetailModel.postQuestionDetail.HashTags);
      

    });
  }
   

  private getAllSliderQuestionlist(qid: number, hashtags: string) {
     


    this.userService.getSimilarQuestionsList(qid, hashtags).subscribe(data => {

      this.PercentageCalc(data);
      this.sliderData = data
       
 
    },
      error => {
      });
  }

  

  private PercentageCalc(data) {
    let scoreYes = 0;
    let scoreNo = 0;
    let _score = 0;
    data.map((x) => {

      if (x.Comments) {

        x.Comments.map(y => {

          _score = y.LikesCount - y.DislikesCount;
          if (y.IsAgree) {
            scoreYes = scoreYes + (_score > 0 ? _score : 0);
          }
          else {
            scoreNo = scoreNo + (_score > 0 ? _score : 0);
          }
        });
      }

      x.percentage = +((scoreYes / (scoreYes + scoreNo)) * 100);
      if (isNaN(x.percentage))
        x.percentage = 0;
      scoreYes = 0;
      scoreNo = 0;
    })
  }

  saveLikeclick(Likes, index) {

    if (this.PostQuestionDetailModel.comments[index].CommentedUserId == this.localStorageUser.Id) { 

      this.toastr.error('', 'You cant vote on your own beliefs.', { timeOut: 5000 });
     return;
  }
    if (this.localStorageUser.Id== 133) {
      this.toastr.error('', 'You cant vote as an anonyomous user.', { timeOut: 5000 });
  return
  }

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
      this.mixpanelService.track('Liked');
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
      this.mixpanelService.track('Unlike');

    }
  }



  saveDislikeclick(DisLikes, index) {

    if (this.PostQuestionDetailModel.comments[index].CommentedUserId == this.localStorageUser.Id) {

      this.toastr.error('', 'You can\'t vote on your own beliefs.', { timeOut: 5000 });
      return;
    }
    if (this.localStorageUser.Id == 133) {
      this.toastr.error('', 'You cant vote as an anonyomous user.', { timeOut: 5000 });
      return
    }
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
      this.mixpanelService.track('Dislike');
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
      this.mixpanelService.track('UnDislike');

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
