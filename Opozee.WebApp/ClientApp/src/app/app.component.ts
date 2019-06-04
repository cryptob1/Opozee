import { Component, OnInit } from '@angular/core';
import { PushNotificationsService } from 'ng-push';
import { UserService } from './_services';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'app';
  localStorageUser: any;
  constructor(private userService: UserService, private _pushNotifications: PushNotificationsService) {
    this._pushNotifications.requestPermission();
  }
  
  ngOnInit() {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));
   // setTimeout(function push() {
      this.userService.getPushNotification()
        .subscribe(data => {

          try {

            //var now = new Date();
            //var delay = 60 * 60 * 1000; // 1 hour in msec
            //var start = delay - (now.getMinutes() * 60 + now.getSeconds()) * 1000 + now.getMilliseconds();

            //setTimeout(function push() {
            //  if (data && this.localStorageUser.Id) {
            //    let options = {
            //      body: data.body,
            //      icon: data.icon
            //    }
            //    this._pushNotifications.create(data.title, options).subscribe(
            //      res => console.log(res),
            //      err => console.log(err)
            //    );
            //  }
            //  setTimeout(push, delay);
            //}, start);



            let options = {
              body: data.body,
              icon: data.icon
            }
            this._pushNotifications.create(data.title, options).subscribe(
              res => console.log(res),
              err => console.log(err)
            );
            
          } catch (err) { }
        },
          error => {
          });

    //}, 5000); //1000 * 60 * 60);
  }


}
