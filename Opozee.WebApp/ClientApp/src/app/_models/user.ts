export class User {
  id: number;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  info: string;
}


export class Question {

  addQuestion: string;
  addHasTag: string;

}


export class LocalStorageUser {



  Id: number;
  email: string;
  UserName: string;
  ImageURL: string;


  constructor() {
    this.Id = 0;
    this.email = '',
      this.UserName = ''

  }
}


export class NotificationsModel {

  Id: Number;
  Username: string;
  Image: string;
  Like: boolean;
  Message: string;
  ModifiedDate: Date;
  Opinion: Date;
  OpinionId: number;
  TotalRecordcount: number;
  IsAgree: boolean;
  OpinionList: any;
  RefferalStatus: boolean;
  QOCreationDate: Date;
  CommentedUserId: number;
}

export class Following {
  id: number;
  UserId: number;
  Following: number;
  IsFollowing: boolean;
}

export class FollowerUser {
  UserID: number;
  FollowerId: number;
  UserName: string;
  ImageURL: string;
  IsFollowing: boolean;
}


export class BookMarkQuestion {
  public comments: Comments[] = [];
  public postQuestionDetail: PostQuestionDetail[] = [new PostQuestionDetail()];
  public IsBookmark: boolean;
  OwnerUserName: string

}

export class BookMarkQuestionVM {
  public comments: Comments[] = [];
  public postQuestionDetail: PostQuestionDetail; //[] = [new PostQuestionDetail()];
  public IsBookmark: boolean;
  OwnerUserName: string
}

export class Comments {

  Comment: string;
  CommentedUserId: number;
  CommentedUserName: string;
  CreationDate: Date;
  DisLikes: boolean;
  DislikesCount: number;
  Id: number;
  IsAgree: boolean;
  Likes: boolean;
  LikesCount: number;
  Name: string;
  UserImage: string;
  LongForm: string;
  BeliefImage: string;

}

export class PostQuestionDetail {
  BookmarkId: number;
  CreationDate: Date;
  HashTags: string;
  Id: number;
  IsBookmark: boolean;
  IsUserPosted: boolean;
  OwnerUserID: number;
  OwnerUserName: string;
  Question: string;
  TotalDisLikes: number;
  TotalLikes: number;
  UserImage: string;
  YesCount: number;
  NoCount: number;
  IsSlider: boolean;
  //MostNoLiked: Comments;
  //MostYesLiked: Comments;
  comments: Comments[] = [];
  percentage: number;
  LastActivityTime: Date;
  constructor() {
    //this.IsBookmark = false;
  }



}


export class UserEarnModel {
  Id: number;
  OwnerUserName: string;
  Earnings: number;

}


export class UserProfileModel {
  BalanceToken: number;
  Email: string;
  FirstName: string;
  ImageURL: string;
  LastName: string;
  Password: string;
  TotalDislikes: string;
  TotalLikes: number;
  TotalReferred: number;
  TotalPostedQuestion: number;
  TotalPostedBeliefs: number;
  UserID: number;
  UserName: string;
  HasFollowed: boolean;
  Followers: number;
  Followings: number;
  UserInfo: string;
}


export class UserEditProfileModel {
  Email: string;
  FirstName: string;
  ImageURL: string;
  LastName: string;
  Password: string;
  UserID: number;
  UserInfo: string;
}
