import { Component, OnInit } from '@angular/core';
import { PushNotificationsService } from 'ng-push';
import { UserService } from './_services';
import 'rxjs/add/observable/interval';
import { Observable } from 'rxjs/Observable';
import { Toast, ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'app';
  localStorageUser: any;
  constructor(private userService: UserService, private _pushNotifications: PushNotificationsService, private toaster: ToastrService) {
    this._pushNotifications.requestPermission();
  }
  
  ngOnInit() {
    this.localStorageUser = JSON.parse(localStorage.getItem('currentUser'));

    //Observable.interval(1000*60)
    //  .subscribe((val) => {
    //    this.userService.getPushNotification()
    //      .subscribe(data => {

    //        try {

    //          //var now = new Date();
    //          //var delay = 60 * 60 * 1000; // 1 hour in msec
    //          //var start = delay - (now.getMinutes() * 60 + now.getSeconds()) * 1000 + now.getMilliseconds();

    //          //setTimeout(function push() {
    //          //  if (data && this.localStorageUser.Id) {
    //          //    let options = {
    //          //      body: data.body,
    //          //      icon: data.icon
    //          //    }
    //          //    this._pushNotifications.create(data.title, options).subscribe(
    //          //      res => console.log(res),
    //          //      err => console.log(err)
    //          //    );
    //          //  }
    //          //  setTimeout(push, delay);
    //          //}, start);

    //          let options = {
    //            body: data.body,
    //            icon: data.icon
    //          }
    //          this._pushNotifications.create(data.title, options).subscribe(
    //            res => console.log(res),
    //            err => console.log(err)
    //          );

    //        } catch (err) { }
    //      },
    //        error => {
    //        });
    //  });


    if (this.localStorageUser) {
      if (this.localStorageUser.Id) {
        Observable.interval(1000*60*60*24)
          .subscribe((val) => { this.checkNotification() });

        this.checkNotification()
      }
    }
  }

  checkNotification() {
    try {
      this.userService.checkNotification(this.localStorageUser.Id)
        //.pipe(first())
        .subscribe(data => {
          console.log('checking....');
          if (this.localStorageUser) {
            if (this.localStorageUser.Id) {
              if (data) {
                if (data.notification) {

                  if (data.notification.length > 0) {
                    console.log('checking....', data.notification.length);
                    let body = "";
                    if (data.notification.length == 1) {
                      body = "You have " + data.notification.length + " unread notification. Click here to see.";
                    }
                    else {
                      body = "You have " + data.notification.length + " unread notifications. Click here to see.";
                    }
                    let options = {
                      body: body,
                      icon: "assets/images/OpozeeIcon.png",
                      silent: true,
                      sticky: true,
                    }
                    this._pushNotifications.create("Opozee.com", options).subscribe(
                      res => {
                        if (res.event.type === 'click') {
                          window.open("https://opozee.com/notifications", "_blank");
                          res.notification.close();
                        }
                      },
                      err => console.log(err)
                    );
                  }
                }


              }
            }
          }
        },
          error => {
            console.log('error: ', error)
          });
    } catch (err) { }
  }
  


  

}
