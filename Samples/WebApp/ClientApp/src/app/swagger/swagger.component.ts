import {AfterViewInit, Component, ElementRef} from '@angular/core';

// import SwaggerUI from 'swagger-ui';
const SwaggerUI = require("swagger-ui")

@Component({
  selector: 'app-swagger',
  templateUrl: './swagger.component.html',
  styleUrls: ['./swagger.component.css']
})
export class SwaggerComponent implements AfterViewInit {

  constructor(private el: ElementRef) {
  }

  ngAfterViewInit() {
    const ui = SwaggerUI({
      dom_id: '#myDomId',
      url: '/swagger/v1/swagger.json',
      domNode: this.el.nativeElement.querySelector('.swagger-container'),
      deepLinking: true,
      presets: [
        SwaggerUI.presets.apis
      ],
    });
  }
}