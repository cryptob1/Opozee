 import { AfterViewChecked, Directive, ElementRef } from '@angular/core';

  @Directive({
    selector: '[ExternalLink]'
  })
  export class ExternalLinkDirective implements AfterViewChecked {
    constructor(private el: ElementRef) { }

    ngAfterViewChecked() {
      Array.from(this.el.nativeElement.querySelectorAll('a'))
        .forEach((el: any) => {
          el.setAttribute('target', '_blank');
        });
    }
  }
