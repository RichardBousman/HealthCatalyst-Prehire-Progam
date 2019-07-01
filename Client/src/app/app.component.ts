/**
 * Manage the Initial (Main) Component for this UI
 */


import { Component, ChangeDetectorRef, ChangeDetectionStrategy, ɵCompiler_compileModuleSync__POST_R3__ } from '@angular/core';
import { PeopleManager } from './ServerInterfaces/PeopleManager';
import { Person } from './model/person';

import { MatSnackBar, MatSnackBarConfig } from "@angular/material" ;
import { AriaLivePoliteness } from '@angular/cdk/a11y';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styles: [
    `
      .list-group-item:first-child {
        border-top-left-radius: 0;
        border-top-right-radius: 0;
        border-top: 0;
      }
    `
  ],
  changeDetection: ChangeDetectionStrategy.Default
 })
export class AppComponent {
  query: string;
  runStatus: string;
  request: XMLHttpRequest;
  people: Array<Person>;
  showAddDialog: boolean;
  currentPersonElement: HTMLElement;
  dataLoaded: boolean;
  simulateDelayFromServer: boolean;

  /**
   * Show the Details for the person selected
   * @param item Person Selected
   */
  showDetails(item) {
    this.query = item.name;

    this.people.forEach ( person => { person.highlight = false ;});

    item.highlight = true ;
    item.showDetails = true ;
    this.currentPersonElement = document.getElementById (`Person${item.personId}`);
  }

  /**
   * Show a Status message to keep the user informed
   * @param message Text of message
   * @param timeout How long status is to be shown in ms (optional, default 2000)
   */
  public showStatus ( message: string, timeout: number = 2000 )
  {
    this.showData ( message, timeout ) ;
  }

  /**
   * Show an Error Message to the user
   * @param message Text of message
   * @param timeout How long error is to be shown in ms (optional, default 4000)
   */
  public showError ( message: string, timeout: number = 4000 )
  {
    this.showData ( message, timeout, 'assertive', 'snackbarError' );
  }

  /**
   * Show the actual message (status or error or...) to the user
   * @param message Text of message
   * @param timeout How long to show the message
   * @param politeness One of the politeness's of the message ('polite' (default), 'assertive', 'none')
   * @param cssStyle Style to use to display the message (optional, default is 'snackbarStyle')
   */
  private showData ( message: string, timeout: number, politeness: AriaLivePoliteness = 'polite', cssStyle: string = 'snackbarStyle') {

    let config: MatSnackBarConfig = new MatSnackBarConfig();

    config.verticalPosition = 'bottom';
    config.horizontalPosition = 'left' ;
    config.announcementMessage = message ;
    config.duration = timeout ;
    config.politeness = politeness;
    config.panelClass = [cssStyle];

    let snackbarRef = this.snackBar.open ( message, null, config );
  }

  /**
   * Constructor
   * @param changeEnforcer Injected to allow forcing updates to the UI
   * @param snackBar Injected Popup status dialog manager
   */
  constructor(private changeEnforcer:ChangeDetectorRef, public snackBar: MatSnackBar) {
    this.query = '';
    this.runStatus = 'Sending Request';
    this.people = [];
    this.showAddDialog = false ;
    this.dataLoaded = false ;
    
    /*VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV*/

    this.simulateDelayFromServer = true ;  // Set true to simulate a delay, otherwise false

    /* ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ */

    PeopleManager.GetPeople ( (peopleSet) => {

        let timeout:number = 0 ;

        if ( this.simulateDelayFromServer )
        {
          timeout = 8000 ;
        }

        setTimeout ( ()=> {
          this.people = peopleSet ;
          this.people.forEach ( person => { person.showDetails = false ;});
          this.dataLoaded = true ;
        }, timeout );
    })
  }

  /**
   * Force the UI to check for changes
   */
  forceUiUpdate() {
    this.changeEnforcer.detectChanges();
    this.scrollCurrentIntoView();
  }

  /**
   * Scroll the current person into view.
   */
  scrollCurrentIntoView() {
    console.log ("ScrollCurrentIntoView...");

    if ( this.currentPersonElement != null )
    {
      this.currentPersonElement.scrollIntoView({ behavior: "smooth", block: "start" }) ;
    }
  }
}
