/**
 * Handling the Editing of a Person's data
 */

import { Component, OnInit, Input, OnChanges } from '@angular/core';
import { AppComponent } from '../app.component';
import { ImageManager } from '../ServerInterfaces/ImageManager';
import { PeopleManager } from '../ServerInterfaces/PeopleManager';
import { FileUploader } from 'ng2-file-upload';
import { Person } from '../model/person';
import { InterestsUpdates } from '../Interests/InterestUpdates';

@Component({
  selector: 'app-PersonEdit',
  templateUrl: './PersonEdit.component.html',
  inputs: ['person']
})
export class PersonEditComponent implements OnInit {
  person: Person;
  parent: AppComponent;
  imageToAssign: string;
  newInterests: InterestsUpdates;

  public uploader: FileUploader = new FileUploader({url: ImageManager.ImageInterfaceUrl, itemAlias: 'photo', autoUpload: true});

  /**
   * Component Constructor
   * @param parentApp Injected parent component
   */
  constructor(parentApp: AppComponent) {
    console.log("Person Edit Constructor Called");
    this.parent = parentApp ;
    }
  
  /**
   * Get the URL of the image (from the server database) to display
   */
  get GetImageUrl() : string {
      return ImageManager.GetImageUrl (this.imageToAssign);
  }

  /**
   * User selected a New Image file
   * @param control UI Control where user selected new image
   */
  newImageSelected ( control ) {
    var newFileName:string = control.currentTarget.files[0].name;

    this.parent.showStatus ( `New File Selected (${newFileName}).`);
  }

  /**
   * Save the Changes to the Person's data.  Determines what fields
   * have been changed and sends the set of changes to the Server.
   * @param formdata Values from UI that represent the changes.
   */
  onSave(formdata) {

    var changes = "" ;

    this.parent.showStatus ( `Saving data for ${this.person.firstName} ${this.person.lastName}`);

    changes = ConcatChanges (formdata.newFirst, this.person.firstName, "firstName", changes );
    changes = ConcatChanges (formdata.newLast, this.person.lastName, "lastName", changes );
    changes = ConcatChanges (formdata.addressLine1, this.person.addressLine1, "addressLine1", changes );
    changes = ConcatChanges (formdata.addressLine2, this.person.addressLine2, "addressLine2", changes );
    changes = ConcatChanges (formdata.city, this.person.city, "city", changes ) ;
    changes = ConcatChanges (formdata.state, this.person.stateOrTerritory, "state", changes ) ;
    changes = ConcatChanges (formdata.country, this.person.country, "country", changes );
    changes = ConcatChanges (formdata.zip, this.person.zipCode, "zip", changes );
    changes = ConcatChanges (this.imageToAssign, this.person.imageGUID, "imageGuid", changes );

    // Add Interest Changes
    let newInterests: string = this.newInterests.MakeInterestList () ;

    if ( newInterests.length > 0 )
    {
      if ( changes.length > 0 ) {
        changes = changes + "@" ;
      }

      changes = changes + newInterests ;
    }

    // Send to server
    if ( changes.length > 0 )
    {
      var parameters = `personId=${this.person.personId}&changes=${changes}`;

      PeopleManager.UpdatePerson ( this.person.personId, changes, (newPerson) => {

              this.parent.showStatus ( `Data saved for ${newPerson.firstName} ${newPerson.lastName}`);

               // Copy 'calculated' fields from server to Person object
               this.person.addressAsHTML = newPerson.addressAsHTML ;
               this.person.addressAsText = newPerson.addressAsText ;
               this.person.addressCommaSeparated = newPerson.addressCommaSeparated ;
               this.person.interests = newPerson.interests ;
      }) ;

      // Copy changes to original Person object

      this.person.firstName = formdata.newFirst ;
      this.person.lastName = formdata.newLast ;
      this.person.addressLine1 = formdata.addressLine1 ;
      this.person.addressLine2 = formdata.addressLine2 ;
      this.person.city = formdata.city ;
      this.person.stateOrTerritory = formdata.state ;
      this.person.country = formdata.country ;
      this.person.zipCode = formdata.zip ;
      this.person.imageGUID = this.imageToAssign ;
    }
    else
    {
      this.parent.showStatus ( "No changes found");
    }

    // If the newText is different from the oldtext, then flag the field
    // as changed and append this change to the set of changes already
    // found.
    function ConcatChanges (newText: string, oldText: string, fieldName: string, oldChanges: string) { 

      // IF there is a change to the field's valud
      if ( newText != oldText )
      {
        var leadingText ;

        if ( oldChanges.length > 0 ){
          leadingText = "@" ;
        }
        else
        {
          leadingText = "";
        }

        // Return the old changes with this change concatenated to them
        return oldChanges + leadingText + fieldName + "=" + newText ;
      }
      else
      {
        return oldChanges;
      }
    }
  }
   
  /**
   * Cancel the editing of the Person's details.  Unsaved changes will
   * be lost.
   */
  cancelEditDetails() {
    
    this.parent.showStatus ( "Canceling Edit, unsaved changes lost." );

    // Remove image to assign if not assigned
    if ( this.imageToAssign != this.person.imageGUID ) 
    {
      ImageManager.DeleteImage ( this.imageToAssign ) ;
    }
 
    var parent: AppComponent = this.parent ;

    if ( parent == null || parent.people == null || parent.people.length == null )
    {
      return ;
    }

    for (var index:number = 0 ; index < parent.people.length; index++ )
    {
        if ( parent.people[index].personId == this.person.personId )
        {
          parent.people[index].editDetails = false ;
        }
    }
  }

  /**
   * Perform final initialization of this component after UI elements created.
   */
  ngOnInit() {

    this.imageToAssign = this.person.imageGUID ;

    // When new image selected during Add, this is called after image added
    this.uploader.onCompleteItem = (item: any, response: any, status: any, headers: any) => {

        var oldImageGuid: string = this.imageToAssign ;

        // Update image on UI

        this.imageToAssign = response ;

        // if the image being displayed isn't needed anymore then delete it from server
        if ( (oldImageGuid != this.imageToAssign) && (oldImageGuid != this.person.imageGUID) )
        {
          ImageManager.DeleteImage ( oldImageGuid ) ;
        }
  
        this.parent.forceUiUpdate();

        this.parent.showStatus ( "Image Updated." );
      }

    this.parent.scrollCurrentIntoView();

    this.parent.showStatus ( `Editing ${this.person.firstName} ${this.person.lastName}`);
  }
}
