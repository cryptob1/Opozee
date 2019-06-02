import { Component, Input, OnInit, ViewChild, Output, EventEmitter} from '@angular/core';
//import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ModalDirective } from 'ngx-bootstrap';

@Component({
  selector: 'confirmation-dialog',
  templateUrl: 'confirmationdialog.component.html'

})
export class ConfirmationDialogComponent implements OnInit {

  @ViewChild('confirmationDialogComponent', { static: true }) public confirmationDialogComponent: ModalDirective;
  @Output() event: EventEmitter<any> = new EventEmitter<any>();

  //@Input() title: string;
  //@Input() message: string;
  //@Input() btnOkText: string;
  //@Input() btnCancelText: string;

  constructor() { }

  ngOnInit() {
  }

  //public decline() {
  //  this.activeModal.close(false);
  //}

  //public accept() {
  //  this.activeModal.close(true);
  //}

  //public dismiss() {
  //  this.activeModal.dismiss();
  //}
  close() {
    this.confirmationDialogComponent.hide();
  }
  show(question?: any): void {
    
    this.confirmationDialogComponent.show();
  }

  confirm() {
    this.confirmationDialogComponent.hide();
    this.event.emit('hello');
  }
}
