import { Injectable } from '@angular/core';

@Injectable()
export class AppConfigService {

  public coinPerReferral = 10;
  public baseURL = "http://localhost:61545/";
   //public baseURL = "http://3.18.212.192:81/"; 
  //public baseURL = "https://opozee.com:81/";
}
