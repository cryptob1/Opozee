import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RequestOptions } from '@angular/http';
import { Http, Headers, Response } from '@angular/http';
import { HttpHeaders } from '@angular/common/http';
import { User, Question, NotificationsModel, PostQuestionDetail, BookMarkQuestion, UserProfileModel } from '../_models';
import { UserEditProfileModel, LocalStorageUser } from '../_models/user';
import { QuestionListing } from '../_models/question';
import { config } from 'process';
import { AppConfigService } from '../appConfigService';
import { Notification, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class UserService {

  _authorizationHeader: string;
  myAppUrl: string = ""
  constructor(private http: HttpClient, appConfigService: AppConfigService) {
    //let headers: Headers = new Headers();

    //headers.append('Access-Control-Allow-Origin', '*');
    //headers.append('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE');
    //headers.append('Authorization', 'Basic b3Bvc2VlOm9wb3NlZTk5IQ==');
    //let options = new RequestOptions({ headers: headers });

    let _currentUser = JSON.parse(localStorage.getItem('currentUser'));
    if (_currentUser)
      if (_currentUser.AuthToken)
        this._authorizationHeader = _currentUser.AuthToken.token_type + ' ' + _currentUser.AuthToken.access_token;

    this.myAppUrl = appConfigService.baseURL;
  }

  private getAuthorizationHeader(): string {
    let _currentUser = JSON.parse(localStorage.getItem('currentUser'));
    if (_currentUser)
      if (_currentUser.AuthToken)
        this._authorizationHeader = _currentUser.AuthToken.token_type + ' ' + _currentUser.AuthToken.access_token;

    return this._authorizationHeader ? this._authorizationHeader : 'Basic b3Bvc2VlOm9wb3NlZTk5IQ=='
  }

  //const httpOptions = {
  //  headers: new HttpHeaders({
  //    'Content-Type': 'application/json',
  //    'Authorization': 'my-auth-token'
  //  })
  //}

  getPushNotification() {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/PushNotification', {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  getAllquestion() {
    return this.http.get<QuestionListing[]>(this.myAppUrl + 'opozee/api/WebApi/GetQuestion', {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  getUserById(id: number) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetUserById' + id, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  getById(id: number) {
    return this.http.get(this.myAppUrl + 'users/ + id');
  }

  register(user: any) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/RegisterUser', user, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  update(user: User) {
    return this.http.put(this.myAppUrl + 'users/' + user.id, user);
  }

  delete(id: number) {
    return; //this.http.delete(`${config.apiUrl}/users/` + id);
  }


  getUserRecords() {
    return this.http.get<QuestionListing[]>(this.myAppUrl + 'opozee/api/WebApi/GetUserALLRecords', {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }


  //Get Popularhastag
  getPopularHasTags() {
    return this.http.get<any[]>(this.myAppUrl + 'opozee/api/WebApi/GetPopularHashTags', {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  checkDuplicateQuestions(question) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/CheckDuplicateQuestions', question, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  //CheckDuplicateBelief
  CheckDuplicateBelief(belief) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/CheckDuplicateBelief', belief, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }


  checkReferralCode(code: string) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/CheckReferralCode?referralCode=' + code, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }
    

  postQuestionweb(question) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/postquestionweb', question, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }


  getAllNotification(Model) {
    return this.http.post<NotificationsModel[]>(this.myAppUrl + 'opozee/api/WebApi/GetAllNotificationByUser',Model, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  getTabOneNotification(Model) {
    return this.http.post<NotificationsModel[]>(this.myAppUrl + 'opozee/api/WebApi/GetProfileNotificationByUser', Model, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
    }

    deleteMyQuestion(model) {
        return this.http.post<NotificationsModel[]>(this.myAppUrl + 'opozee/api/WebApi/DeleteMyQuestion', model, {
            headers: new HttpHeaders({
                'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
            })
        })
    }

    deleteMyBelief(model) {
        return this.http.post<NotificationsModel[]>(this.myAppUrl + 'opozee/api/WebApi/DeleteMyBelief', model, {
            headers: new HttpHeaders({
                'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
            })
        })
    }


  //getAllQuestionlist(questionGetModel) {
  //  return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllPostsWeb?UserId=' + userId + '&Search=' + search +'&pageNumber'+, {
  //    headers: new HttpHeaders({
  //      'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()

  //    })

  //  })
  //}

  getBountyQuestions(startDate, endDate) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetBountyQuestions?StartDate='
      + startDate + '&EndDate=' + endDate, {
        headers: new HttpHeaders({
          'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
        })
      })
  }

  getTopEarners(days: number) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetTopEarners?days=' + days ,  {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }


  getAllQuestionlist(questionGetModel) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllPostsWeb', questionGetModel, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }

  getSimilarQuestionsList(qid: number, tags: string) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetSimilarQuestionsWeb?qid='+qid+'&tags='+tags , {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }

  getAllSliderQuestionlist(questionGetModel) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllSliderPostsWeb', questionGetModel, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }


  getquestionDetails(Id: number, UserId: number) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllOpinionWeb?questId=' + Id + '&UserId=' + UserId, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    }).pipe(map(data => { return data }
    ))
  }


  getUserProfileWeb(userid: number) {
    return this.http.get<UserProfileModel>(this.myAppUrl + 'opozee/api/WebApi/GetUserProfileWeb?userid=' + userid, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    }).pipe(map(profiledata => { return profiledata }

    ))
  }

  editUserprofile(Model): Observable<any> {
    return this.http.post(this.myAppUrl + 'opozee/api/WebApi/EditUserProfileWeb', Model, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }

  uploadfileService(userId, fileToUpload: File) {
    debugger;
    const formData: FormData = new FormData();
    formData.append('Image', fileToUpload, fileToUpload.name);
    formData.append('userId', userId);
    return this.http.post(this.myAppUrl + 'opozee/api/WebApi/UploadProfileWeb', formData, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }


  //UserEditProfileModel
  getEditUserProfileWeb(userid: number) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetEditUserProfileWeb?userid=' + userid, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    }).pipe(map(profiledata => { return profiledata }

    ))
  }

    checkNotification(userId) {
        return this.http.get<any[]>(this.myAppUrl + 'opozee/api/WebApi/CheckNotification?userId=' + userId, {
            headers: new HttpHeaders({
                'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
            })
        })
    }


  saveOpinionPost(Model) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/PostOpinionWeb', Model, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }


  //UserEditProfileModel
  GetAllTaggedDropService() {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllTaggedDropWeb', {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    }).pipe(map(profiledata => { return profiledata }

    ))
  }

  //savelike or dislkie
  SaveLikeDislikeService(Model) {
    return this.http.post(this.myAppUrl + 'opozee/api/WebApi/PostLikeDislikeWeb', Model, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })

  }

  saveBookmarkQuestionservice(Model) {

    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/BookMarkQuestionWeb', Model, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })

    })
  }


  getAllBookMarkService(userid: number) {

    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllBookMarkWebById/?userid=' + userid, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })

    })
  }

  getquestionDetails1(Id: number, UserId: number) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllOpinionWeb?questId=' + Id + '&UserId=' + UserId, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    }).pipe(map(data => { return data }
    ))
  }

  getQuestionListEditService(Model) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/GetAllPostsQuestionEditWeb' , Model , {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    }).pipe(map(data => { return data }
    ))
  }
  

  getpostedQuestionwebService(QuestionId) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/GetPostedQuestionEditWeb?QuestionId=' + QuestionId, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  editPostQuestionwebService(question) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/EditPostQuestionWeb', question, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  deletePostQuestionwebService(question) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/DeletePostQuestionWeb', question, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  sendContactMail(contact) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/SendContactMail', contact, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  sendWelcomeMail(contact) {
    return this.http.post<any>(this.myAppUrl + 'opozee/api/WebApi/SendWelcomMail', contact, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

  emailVerification(userId, code) {
    return this.http.get<any>(this.myAppUrl + 'opozee/api/WebApi/EmailVerification?Id=' + userId + '&Code=' + code, {
      headers: new HttpHeaders({
        'Authorization': this._authorizationHeader ? this._authorizationHeader : this.getAuthorizationHeader()
      })
    })
  }

}
