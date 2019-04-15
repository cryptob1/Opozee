import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'postdialogbelief',
  templateUrl: './postdialogbelief.component.html',

})

export class PostDailogBelief implements OnInit {


  constructor(private route: ActivatedRoute, public dialog: MatDialog
  ) {
    
  }

  //logout() { }
  //toggle() {
  //  this.isExpanded = !this.isExpanded;
  //}

  ngOnInit() {

    //alert(123);
  }
}
