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
import { PopoverModule } from "ngx-popover";
import { Observable } from 'rxjs';
import { ModalModule } from 'ngx-bootstrap/modal';
import { Meta } from '@angular/platform-browser';

enum Reaction {
  Thoughtful = 1,
  Factual = 2,
  Funny = 3,
  Irrational = 4,
  Fakenews = 5,
  OffTopic = 6
}


@Component({
  selector: 'questiondetail-component',
  templateUrl: './questiondetail.component.html',
  styleUrls: ['./questiondetail.component.css']
})

export class Questiondetail implements OnInit {

  @ViewChild('dialogPostBelief') dialogPostBelief: DialogPostBelief;
 
  showReaction = false;
  model: any = {};
  postOpinionForm: FormGroup;
  loading = false;
  returnUrl: string;
  isAuthenticate: boolean;
  Id: number;
  Isclicked: boolean = false;
  comment: '';
  submitted: boolean = false;
  imageShowLike: number = -1;
  imageShowDislike: number = -1;
  expand: boolean = false;
  animal: string;
  name: string;
  countReactionScore: number;
  PostQuestionDetail: any;
  tweetText: string = '';//'Hey I’d like get your take on this question –';
  shareUrl: string;
  public _emitter: EventEmitter<any> = new EventEmitter();
  encoder: HttpUrlEncodingCodec = new HttpUrlEncodingCodec();

  sliderData: PostQuestionDetail[] = [];
  
  display = 'none';


  dataModel = {
    'QuestId': 0, 'Comment': '',
    'CommentedUserId': 0,
    'Likes': 0,
    'OpinionAgreeStatus': 0,
    'Dislikes': 0,
    'CommentId': 0,
    'CreationDate': new Date(),
    'LikeOrDislke': false,
    'ReactionType':0
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
    private mixpanelService: MixpanelService,
    private meta: Meta
  ) {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
  
    
    if (this.route.snapshot.params["Id"]) {
      this.Id = this.route.snapshot.params["Id"];
    }


    this.shareUrl = "https://opozee.com/qid/" + (this.Id);
   // this.tweetText = this.html2text(this.PostQuestionDetailModel.postQuestionDetail.Question) + " - See opposing views at ";

    //this.shareUrl = window.location.origin + this.location.path();
    this.router.routeReuseStrategy.shouldReuseRoute = function () {
      return false;
    };
 
    if (this.localStorageUser != null) { 
      mixpanelService.init(this.localStorageUser['Email'])
    }
    
    
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
    this.mixpanelService.track('questiondetail:' + this.Id);


 
  }

  percentage: number = 0;

  getQuestionDetail(fromLikeBtn?) {
    
    this.userService.getquestionDetails(this.Id, this.localStorageUser.Id).subscribe(data => {
      
      this.PostQuestionDetailModel = data as BookMarkQuestionVM;
      this.PostQuestionDetailModel.comments = data['Comments'] as Comments[];

      //console.log(this.PostQuestionDetailModel.comments);
 
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
      this.tweetText = this.html2text(this.PostQuestionDetailModel.postQuestionDetail.Question) + " - See opposing views at ";
      
      //console.log(this.tweetText);
      
      this.meta.updateTag({ name: 'description', content: this.html2text(this.PostQuestionDetailModel.postQuestionDetail.Question) });


      // <!-- Facebook meta data -->
      this.meta.addTags([
        { property: 'og:title', content: this.html2text(this.PostQuestionDetailModel.postQuestionDetail.Question) },
        { property: 'og:url', content: this.shareUrl },
      ]);
 



      //console.log(this.PostQuestionDetailModel);
      if (!fromLikeBtn)
        this.getAllSliderQuestionlist(this.Id, this.PostQuestionDetailModel.postQuestionDetail.HashTags);
    });
  }
   

  private getAllSliderQuestionlist(qid: number, hashtags: string) { 
    this.userService.getSimilarQuestionsList(qid, hashtags).subscribe(data => {
      this.PercentageCalc(data);
      this.sliderData = data;
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

  private checkTooManyLikes() {
    let now = Date.now().valueOf();
    let count = 0;
    let lastVoteTime = new Date(JSON.parse(localStorage.getItem('lastVoteTime'))).valueOf();
 
    if ((now - lastVoteTime ) < 2 * 60 * 1000) {
      count = +localStorage.getItem('voteCount') ;

      localStorage.setItem('voteCount', count + 1+"");
      if (count > 5) {
        return false;
      }
    }
    else {
    localStorage.setItem('voteCount', "1");
    localStorage.setItem('lastVoteTime', now.toString());
    }
    return true;
  }


  saveLikeclick(Likes, index, qDetail?,reactionType?,pop?) {
    debugger
    //alert(reactionType);
    if (!this.checkTooManyLikes()) {
      this.toastr.error('', 'Too many Votes. Slow down!', { timeOut: 5000 });
      return;
    }

    if (this.PostQuestionDetailModel.comments[index].CommentedUserId == this.localStorageUser.Id) {
      this.toastr.error('', 'You cant vote on your own beliefs.', { timeOut: 5000 });
      return;
    }

    if (this.localStorageUser.Id == 133) {
      this.toastr.error('', 'You cant vote as an anonyomous user.', { timeOut: 5000 });
      return
    }

    this.loading = true;
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
      this.dataModel.ReactionType = reactionType;
      //this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel, qDetail);
      this.mixpanelService.track('Liked');
      this.loading = true;
      qDetail.LikesCount += 1;
      if (qDetail.DisLikes) qDetail.DislikesCount -= 1;
      qDetail.Likes = true;
      qDetail.DisLikes = false;
      pop.hide();
    }
    else {
      this.imageShowLike = -1;

      this.dataModel.Likes = 0;
      this.dataModel.Dislikes = 0;
      this.dataModel.QuestId = this.Id;
      this.dataModel.Comment = this.comment;
      this.dataModel.CommentedUserId = this.localStorageUser.Id;
      this.dataModel.LikeOrDislke = true;
      this.dataModel.ReactionType = reactionType;
      //this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel, qDetail);
      this.mixpanelService.track('Unlike');
      this.loading = true;
      qDetail.LikesCount -= 1;
      if (qDetail.DisLikes) qDetail.DislikesCount -= 1;
      qDetail.Likes = false;      
      qDetail.DisLikes = false;
      pop.hide();
    }
  }

  saveDislikeclick(DisLikes, index, qDetail?, reactionType?, pop?) {
    if (!this.checkTooManyLikes()) {
      this.toastr.error('', 'Too many Votes. Slow down!', { timeOut: 5000 });
      return;
    }
    if (this.PostQuestionDetailModel.comments[index].CommentedUserId == this.localStorageUser.Id) {

      this.toastr.error('', 'You can\'t vote on your own beliefs.', { timeOut: 5000 });
      return;
    }
    if (this.localStorageUser.Id == 133) {
      this.toastr.error('', 'You cant vote as an anonyomous user.', { timeOut: 5000 });
      return
    }

    this.loading = true;
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
      this.dataModel.ReactionType = reactionType;
      //this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel, qDetail);
      this.mixpanelService.track('Dislike');
      qDetail.DislikesCount += 1;
      if (qDetail.Likes) qDetail.LikesCount -= 1;
      qDetail.Likes = false;
      qDetail.DisLikes = true;
      pop.hide();
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
      this.dataModel.ReactionType = reactionType;
      //  this.Isclicked = true;
      this.dataModel.CommentId = this.PostQuestionDetailModel.comments[index].Id;
      this.dataModel.CreationDate = new Date();
      this.SaveLikeDislike(this.dataModel, qDetail);
      this.mixpanelService.track('UnDislike');
      qDetail.DislikesCount -= 1;
      if (qDetail.Likes) qDetail.LikesCount -= 1;
      qDetail.Likes = false;
      qDetail.DisLikes = false;
      pop.hide();
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
 
  }

  saveOpinionclick() {
    
    this.submitted = true;
    
    if (this.dataModel.Comment == '' || this.dataModel.Comment == undefined) {
      return;
    }


    this.loading = true;
    this.userService.saveOpinionPost(this.dataModel, null)
      .pipe(first())
      .subscribe(data => {
        
        if (data.BalanceToken <= 0) {

          this.toastr.error('Token Blance 0', 'You have 0 tokens in your account. Please email us to refill the account to post opinion.', { timeOut: 5000 });
        }
        else {
          this.getQuestionDetail();
          this.toastr.success('Posted!', '');
        }
       
        this.loading = false;
        this.Isclicked = false;


      },
        error => {
          //this.alertService.error(error);
          //this.loading = false;
          if (error.status == 401) {
            this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
            Observable.interval(1000)
              .subscribe((val) => {
                this.logout();
              });
          }
        });
  }

  searchForTag(hashtag) {
    this.router.navigateByUrl('/questionlistings/' + hashtag, { skipLocationChange: true }).then(() =>
      this.router.navigate(['/questions/', hashtag]));
    //this.router.navigate(['/questions'], { queryParams: { tag: 1 } });
  }


  SaveLikeDislike(dataModel, qDetail) {
    debugger
    this.loading = true;
    this.userService.SaveLikeDislikeService(this.dataModel)
      .pipe(first())
      .subscribe(data => {
        this.loading = false;
    
        //this.toastr.success('Data save successfully', '');
       //this.getQuestionDetail(true);

        try {
          this.userService.getquestionDetails(this.Id, this.localStorageUser.Id)
            .subscribe(data => {
              let __data = data.Comments.filter(x => x.Id === qDetail.Id)
              console.log('updated', data);
              qDetail.LikesThoughtfulCount = __data[0].LikesThoughtfulCount;
              qDetail.LikesFactualCount = __data[0].LikesFactualCount;
              qDetail.LikesFunnyCount = __data[0].LikesFunnyCount;
              qDetail.DislikesIrrationalCount = __data[0].DislikesIrrationalCount;
              qDetail.DislikesFakeNewsCount = __data[0].DislikesFakeNewsCount;
              qDetail.DislikesOffTopicCount = __data[0].DislikesOffTopicCount;
            });
        }
        catch (err) { }

        this.dataModel = {
          'QuestId': 0, 'Comment': '',
          'CommentedUserId': 0,
          'Likes': 0,
          'OpinionAgreeStatus': 0,
          'Dislikes': 0,
          'CommentId': 0,
          'CreationDate': new Date(),
          'LikeOrDislke': false,
          'ReactionType': 0
        }
        //this.router.navigate(['/questiondetail/', this.Id]);

      },
      error => {
          debugger
          //this.alertService.error(error);
        if (error.status == 401) {
          this.toastr.error('Please Login Again.', error.statusText, { timeOut: 5000 });
          Observable.interval(1000)
            .subscribe((val) => {
              this.logout();
            });
        }
          //this.router.navigate(['login']);
          this.loading = false;
        });
  }

  logout() {
    localStorage.removeItem('currentUser');
    window.location.reload();
  }

  private QuestionDetailsData(data) {
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
    this.tweetText = this.html2text(this.PostQuestionDetailModel.postQuestionDetail.Question) + " - See opposing views at ";

  }


  saveBookmarkQuestion(IsBookmark) {

    
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
 
    this.dialogPostBelief.show(this.dataModel);

  }



}
