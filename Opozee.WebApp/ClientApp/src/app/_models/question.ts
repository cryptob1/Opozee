export class QuestionListing {
  id: number;
  question: string;
  owneruserid: number;
  userName: string;
  hashtags: string;
  creationdate: string;
  ImageURL: string;
  UserID: number;
}


export class detail {
  PostQuestionDetail: object;
  Comments: {};
}



export class PopularHasTags {
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
}
