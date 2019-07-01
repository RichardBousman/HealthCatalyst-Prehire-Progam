/**
 * Manage the Interest add/removal component
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { InterestsUpdates } from './InterestUpdates';
import { Person } from '../model/person';
import { InterestUpdateNode, InterestState } from './InterestUpdateNode';
import { AppComponent } from '../app.component';
import { PersonEditComponent } from '../personedit/PersonEdit.Component';

@Component({
  selector: 'app-Interests',
  templateUrl: './Interests.component.html',
  inputs: ['person', 'parentComponent']
})
export class InterestsComponent implements OnInit {
  person: Person;
  isDataAvailable: boolean;
  interestData: InterestsUpdates ;
  parentComponent: object;
  interestFieldValue: string;

  constructor() {
     this.isDataAvailable = false ;
     this.interestFieldValue = "" ;
  }

  /**
   * Add an Interest to the User
   * @param form Values entered by the user on the UI
   */
  onAddInterest (form) {
    let newInterest:string = form.newInterest ;

    if ( newInterest != undefined && newInterest.length > 0 )
    {
      if ( ! this.interestData.AddInterest ( newInterest ) )
      {
        (this.parentComponent as PersonEditComponent).parent.showError ( `Interest ${newInterest} already exists.`) ;
      }
      this.interestFieldValue="" ;
    }
    else
    {
      (this.parentComponent as PersonEditComponent).parent.showError ( "'Interest' must not be blank.") ;
    }
  }

  /**
   * Remove the Interest from the set of User Interests
   * @param interest Interest to remove
   */
  onRemoveInterest (interest: string) {
    this.interestData.DeleteInterest ( interest ) ;
  }

  /**
   * Finalizse the Configuration of the Element
   */
  ngOnInit() {

    this.interestData = new InterestsUpdates () ;
    this.interestData.Initialize ( this.person ) ;

    (this.parentComponent as PersonEditComponent).newInterests = this.interestData ;

    this.isDataAvailable = true ;
  }
}
