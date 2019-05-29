import { Injectable } from '@angular/core';

@Injectable()
export class AppConfigService {

  public bountyStartDate = '2019-05-23';
  public bountyEndDate = '2019-05-25';
  public coinPerReferral = 5;
  public baseURL = "http://localhost:61545/";
  //public baseURL = "http://3.18.212.192:81/"; 
  //public baseURL = "https://opozee.com:81/";
}
