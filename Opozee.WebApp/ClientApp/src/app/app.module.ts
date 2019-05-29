import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule} from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AppComponent } from './app.component';
import { AlertComponent } from './_directives/index';
import { AuthGuard } from './_guards/index';
import { JwtInterceptor } from './_helpers/index';
import { AlertService, AuthenticationService, UserService } from './_services/index';
import { HomeComponent } from './home/index';
import { LoginComponent } from './login/index';
import { RegisterComponent } from './register/index';
import { routing } from './app.routing'
import { HttpModule } from '@angular/http';
import { NgMultiSelectDropDownModule } from 'ng-multiselect-dropdown';
import { QuestionListingComponent } from './question/QuestionListing';
import { PostQuestionComponent } from './question/PostQuestion';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { NotificationComponent } from './notification/notification.component';
import { AppConfigService } from './appConfigService';
import { HeaderComponent } from './header/header.component';
import { Questiondetail } from './questionDetail/questiondetail.component';
import { ProfileComponent } from './user/profile/profile.component';
import { ViewProfileComponent } from './user/viewProfile/viewprofile.component';
import { EditProfileComponent } from './user/editprofile/editProfile.component';
import { DataSharingService } from './dataSharingService';
import { ConfigsSocial } from './ConfigsSocial ';
import { UserpostQuestion } from './user/userpostedQuestion/userpostQuestion.component';
import { PostedQuestionEditList } from './question/PostedQuestion/postedQuestionEditList.component';
import { EditPostquestion } from './question/editPosttedQuestion/editPostquestion.component';

import { TimeAgoPipe } from 'time-ago-pipe';
import { Ng2EmojiModule } from 'ng2-emoji';


import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import {
  SocialLoginModule,
  AuthServiceConfig,
  GoogleLoginProvider,
  FacebookLoginProvider,
 
} from "angular5-social-login";
import { BookmarkQuestion } from './bookmark/bookmark.component';
import { termandConditionComponent } from './legal/termandCondition.component';

import { privatePolicyComponent } from './legal/privatePolicy.component';
import { aboutusComponent } from './aboutus/aboutus.component';

import { ModalModule, BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { DialogPostBelief } from './questionDetail/dialogPostBelief/dialogPostBelief.component';
import { NgxEditorModule } from 'ngx-editor';
import { AngularFontAwesomeModule } from 'angular-font-awesome';
import { ExternalLinkDirective } from './_helpers/external-link.directive';
import { FaqComponent } from './faq/faq.component';
import { ResetPassword } from './user/resetPassword/resetPassword.component';
import { ForgotPassword } from './user/forgotPassword/forgotPassword.component';
import { EqualValidatorDirective } from './_directives/equal-validator.directive';

// import social buttons module
import { JwSocialButtonsModule } from 'jw-angular-social-buttons';

import { ConfirmationDialogComponent } from './Shared/confirmationDialog/confirmationDialog.component';
import { EarnStatsComponent } from './earn-stats/earn-stats.component' 
import { MixpanelService } from './_services/mixpanel.service';
import { InviteComponent } from './user/invite/invite.component';
import { BountyQuestionsComponent } from './question/bounty-questions/bounty-questions.component';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown'
import { SafeHtmlPipe } from './Shared/safe-html-pipe';

export function getAuthServiceConfigs() {
  let config = new AuthServiceConfig(
    [
      {
        //600720573732817
        id: FacebookLoginProvider.PROVIDER_ID,
        provider: new FacebookLoginProvider("2286238128100867")
      },
      {
        id: GoogleLoginProvider.PROVIDER_ID,
        provider: new GoogleLoginProvider("921095151138-22m1c3n5na9794nprm5e3u24luhorpmn.apps.googleusercontent.com")
      },
    ]
  )
  return config;
}

@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    ReactiveFormsModule,
    HttpModule,
    routing,
    NgMultiSelectDropDownModule.forRoot(),
    BrowserAnimationsModule, // required animations module
    ToastrModule.forRoot(),
    SocialLoginModule,
    Ng2EmojiModule.forRoot(),
    ModalModule.forRoot(),
    NgxEditorModule,
    AngularFontAwesomeModule,
    JwSocialButtonsModule,
    BsDropdownModule.forRoot(),
    
  ],
  declarations: [
    AppComponent,
    AlertComponent,
    HomeComponent,
    LoginComponent,
    RegisterComponent,
    QuestionListingComponent,
    PostQuestionComponent,
    NavMenuComponent,
    NotificationComponent,
    HeaderComponent,
    Questiondetail,
    ProfileComponent,
    ViewProfileComponent,
    EditProfileComponent,
    UserpostQuestion, BookmarkQuestion, PostedQuestionEditList, EditPostquestion, termandConditionComponent
    , privatePolicyComponent,
    TimeAgoPipe,
    DialogPostBelief,
    aboutusComponent,
    ExternalLinkDirective,
    FaqComponent,
    ResetPassword,
    ForgotPassword,
    EqualValidatorDirective,
    ConfirmationDialogComponent,
    EarnStatsComponent,
    InviteComponent,
    BountyQuestionsComponent,
    SafeHtmlPipe
  
  ],
  entryComponents: [
    DialogPostBelief,
    ResetPassword,
    ForgotPassword,
    ConfirmationDialogComponent
  ],
  providers: [
    AuthGuard,
    AlertService,
    AuthenticationService,
    AppConfigService,
    UserService,
    //BsModalService,
    //BsModalRef,
    MixpanelService,
 
    DataSharingService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    },
    {
      provide: AuthServiceConfig,
      useFactory: getAuthServiceConfigs
    }
    // provider used to create fake backend
   
  ],

  bootstrap: [AppComponent]
})

export class AppModule { }

