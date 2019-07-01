/**
 * Component to handle the display of person details.
 */

import { Component, OnInit } from '@angular/core';
import { AppComponent } from '../app.component';
import { ImageManager } from '../ServerInterfaces/ImageManager';
import { PeopleManager } from '../ServerInterfaces/PeopleManager';
import { Person } from '../model/person';

@Component({
  selector: 'app-PersonDetails',
  templateUrl: './PersonDetails.component.html',
  inputs: ['person']
})
export class PersonDetailsComponent implements OnInit {
  parent: AppComponent;
  person: Person;

  /**
   * Component Constructor
   * @param parentApp Injected in Parent Component
   */
  constructor(parentApp: AppComponent) {
  
    this.parent = parentApp ;
  }
  
  /** 
   * Get the URL for the current Person's Image
   * @returns URL to get the person's image from the Server's database
   */
  get GetImageUrl() : string {
     return ImageManager.GetImageUrl (this.person.imageGUID);
  }

  /** 
   * Delete the current person if it is not one of the 'Important' people (as set up by the SeedData).
   */
  deletePerson () {

    if ( this.person.personId == "1" || this.person.personId == "2" )
    {
      this.parent.showError ( `Unable to delete ${this.person.firstName} ${this.person.lastName}.` );

      return ;
    }

    this.parent.showStatus ( `User ${this.person.firstName} ${this.person.lastName} being Deleted.`);

    PeopleManager.DeletePerson ( this.person.personId ) ;

    var peopleArray: Array<Person> = this.parent.people ;

    const index: number = peopleArray.indexOf (this.person, 0);

    if ( index > -1)
    {
      peopleArray.splice(index, 1);
    }

    this.parent.forceUiUpdate ();

    this.parent.showStatus ( `User ${this.person.firstName} ${this.person.lastName} Deleted.`);
  }

  /**
   * Empty, required by OnInit, initialization after UI Elements created.
   */
  ngOnInit () {}
}
