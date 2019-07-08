/**
 * Manage the display of the simple (intial) view of the Person's data
 */

import { Component, OnInit } from '@angular/core';
import { AppComponent } from '../app.component';


@Component({
  selector: 'app-TechInfo',
  templateUrl: './TechInfo.component.html',
  styleUrls: ['./TechInfo.component.css']
})
export class TechInfoComponent implements OnInit {

  /**
   * Component Constructor
   */
  constructor(private parent: AppComponent) { }

  /**
   * Final Initiialization of Component after element created.
   */
  ngOnInit() {}

}