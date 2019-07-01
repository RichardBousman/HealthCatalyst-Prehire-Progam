/**
 * Manage the display of the simple (intial) view of the Person's data
 */

import { Component, OnInit } from '@angular/core';
import { ImageManager } from '../ServerInterfaces/ImageManager';
import { Person } from '../model/person';

@Component({
  selector: 'app-PersonSimple',
  templateUrl: './PersonSimple.component.html',
  inputs: ['person']
})
export class PersonSimpleComponent implements OnInit {
  person: Person;

  /**
   * Component Constructor
   */
  constructor() {
  }

  /**
   * Get the URL of the image to be displayed (URL points to server which accesses from database)
   */
  get GetImageUrl() : string {
    return ImageManager.GetImageUrl (this.person.imageGUID);
  }

  /**
   * Final Initiialization of Component after element created.
   */
  ngOnInit() {}

}
