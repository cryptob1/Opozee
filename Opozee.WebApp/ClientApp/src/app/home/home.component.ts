import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';
import { Router, ActivatedRoute } from '@angular/router';
import { User } from '../_models';
import { UserService } from '../_services';
import { QuestionListing } from '../_models/question';

//import { Emoji } from 'emoji-mart'


@Component({
  templateUrl: 'home.component.html',
  styleUrls: ['home.component.css']
})

export class HomeComponent implements OnInit {
  currentUser: User;
  users: User[] = [];
  questionListing: QuestionListing[] = [];
  popularhastags: any = [];
  showPopup: boolean =true;
  
  constructor(private userService: UserService,private route: ActivatedRoute,
    private router: Router) {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
  

    if (JSON.parse(localStorage.getItem('popupShown')) || this.currentUser != null) {
      this.showPopup = false;
    }
    else {
      
      localStorage.setItem('popupShown', 'true')
    }

  }

  ngOnInit() {
    //this.loadAllUsers();
    //this.getUserALLRecords();
    this.getHastagsRecords();
  }

  hidePopup( ) {
    
    this.showPopup = false;

 

  } 

  popup() {
 
    this.showPopup = true;
    window.scroll(0, 0);

    //document.getElementById('popup-container').classList.add('show');
 
  }


  deleteUser(id: number) {
    //this.userService.delete(id).pipe(first()).subscribe(() => { 
    // this.loadAllUsers() 
    //});
  }

  //seemes to be getting popular questions
  private getUserALLRecords() {
    this.userService.getUserRecords().pipe(first()).subscribe(users => {
      this.questionListing = users;
      console.log(this.questionListing);
    });
  }

  // getting popular Hastags
  private getHastagsRecords() {

   
    this.userService.getPopularHasTags().pipe(first()).subscribe(data => {

      this.popularhastags = data.reduce((arr, _item) => {
        let exists = !!arr.find(x => x.HashTag === _item.HashTag);
        if (!exists) {
          arr.push(_item);
        }
        
        return arr;
      }, []);

    } );
  }



  searchForTag(hashtag) {
    this.router.navigateByUrl('/questionlistings/' + hashtag, { skipLocationChange: true }).then(() =>
      this.router.navigate(['/questions/', hashtag]));
    //this.router.navigate(['/questions'], { queryParams: { tag: 1 } });
  }

}
