import { Component, OnInit, Input, OnChanges } from '@angular/core';
import { AppComponent } from '../app.component';
import {  FileUploader, FileSelectDirective } from 'ng2-file-upload/ng2-file-upload';
import { ImageManager } from '../ServerInterfaces/ImageManager';
import { PeopleManager } from '../ServerInterfaces/PeopleManager';
import { Person } from '../model/person';
import { InterestsUpdates } from '../Interests/InterestUpdates';

@Component({
  selector: 'app-PersonAdd',
  templateUrl: './AddPerson.component.html',
})
export class PersonAddComponent implements OnInit {
   parent: AppComponent;
   imageFileLabel: string;
   imageGuid: string;
   emptyPerson: Person;
   newInterests: InterestsUpdates;

   public uploader: FileUploader = new FileUploader({url: ImageManager.ImageInterfaceUrl, itemAlias: 'photo', autoUpload: true});

   /**
    * Get the Url of the image to display
    */
   get GetImageUrl() : string {
     return ImageManager.GetImageUrl (this.imageGuid);
   }

   /**
    * Constructor
    * @param parentApp Injected parent Component
    */
  constructor(parentApp: AppComponent) {

    this.parent = parentApp ;
    this.imageFileLabel = "No Image";
    this.imageGuid = ImageManager.UnknownImageGuid ;
    this.emptyPerson = new Person() ;
  }

  /**
   * Save the changes entered by the user
   *  1) Make concatentate changes into a string for the server (works like update
   *      where only changed fields are sent to server)
   *  2) Send list of changes to the Server as an POST (Add)
   *  3) Update entry on list of persons locally
   * @param formdata Data entered on the UI
   */
  onSave(formdata) {

    // IF required field
    if ( fieldIsNull ( "First Name", formdata.newFirst, this.parent ) || fieldIsNull ( "Last Name", formdata.newLast, this.parent) ) {
      return ;
    }

    var changes: string = "" ;

    this.parent.showStatus ( `User ${formdata.newFirst} ${formdata.newLast} being added.`);

    changes = ConcatChanges (formdata.newFirst, "firstName", changes );
    changes = ConcatChanges (formdata.newLast, "lastName", changes );
    changes = ConcatChanges (formdata.addressLine1, "addressLine1", changes );
    changes = ConcatChanges (formdata.addressLine2, "addressLine2", changes );
    changes = ConcatChanges (formdata.city, "city", changes ) ;
    changes = ConcatChanges (formdata.state, "state", changes ) ;
    changes = ConcatChanges (formdata.country, "country", changes );
    changes = ConcatChanges (formdata.zip, "zip", changes );
    changes = ConcatChanges (this.imageGuid, "imageGuid", changes )

    // Add Interest Changes
    let newInterests: string = this.newInterests.MakeInterestList () ;

    if ( newInterests.length > 0 )
    {
      if ( changes.length > 0 ) {
        changes = changes + "@" ;
      }

      changes = changes + newInterests ;
    }

    PeopleManager.AddPerson ( changes, (newPersonObject) => {
      this.parent.showStatus ( `User ${formdata.newFirst} ${formdata.newLast} Added.`);
      UpdateParentsPerson ( this, newPersonObject ) ;
    });

    this.parent.showWhat = 'List';

    /**
     * Check to see if the field is null, and if so show message to user and return true
     * @param fieldName Name of field being checked (displayed to the user)
     * @param fieldValue Value user entred
     * @param parent Parent of this component
     */
    function fieldIsNull ( fieldName: string, fieldValue: string, parent: AppComponent ) : Boolean
    {
      if ( fieldValue == undefined || fieldValue == '' )
      {
        parent.showError ( `Field '${fieldName}' must not be null.`);

        return true ;
      }
      else
      {
        return false ;
      }
    }

    /**
     * Add the new field to the set of changes.  This works the same as Update,
     * where only the fields that are changed are sent.  In this case only the fields that
     * the user entered data are sent.
     * @param newText Text user entered
     * @param fieldName Name of field that is to be sent to Server to identify this data
     * @param oldChanges Set of previous changes to add this text to.
     */
    function ConcatChanges (newText: string, fieldName: string, oldChanges: string) { 

      if ( newText == null || newText == "" )
      {
          return oldChanges ;
      }

      var leadingText ;

      if ( oldChanges.length > 0 ){
        leadingText = '@' ;
      }
      else
      {
        leadingText = '';
      }

      // Return the old changes with this change concatenated to them
      return oldChanges + leadingText + fieldName + "=" + newText ;
    }

    // Update entry on parents list of people
    /**
     * Update the data on the parent Component
     * @param ui this component
     * @param newPerson New person data from Server after Add
     */
    function UpdateParentsPerson ( ui, newPerson: Person)
    {
      var parent = ui.parent ;

      if ( parent == null || parent.people == null )
      {
        return ;
      }

      parent.people.push ( newPerson );
      parent.forceUiUpdate();
    }
  }

  /**
   * Cancel the add of a Person
   */
  cancelAdd () {
    this.parent.showStatus ( `User Add Cancelled.`);
 
    var parent: AppComponent = this.parent ;

    // Remove image from database (note unknown image will never be removed)

    ImageManager.DeleteImage ( this.imageGuid ) ;

    if ( parent == null )
    {
      return ;
    }

    parent.showWhat = 'List';
  }

  /**
   * User selected a new Image file
   * @param control Control used to select the image file
   */
  newFileSelected(control)
  {
    var newFileName:string = control.currentTarget.files[0].name;

    this.parent.showStatus ( `New File Selected (${newFileName}).`);
  }

  /**
   * Finish initializing the Element by setting up the callbacks.
   */
  ngOnInit() {
    this.uploader.onAfterAddingFile = (file) =>
    { 
      file.withCredentials = false;
    };

    // When new image selected during Add
    this.uploader.onCompleteItem = (item: any, response: any, status: any, headers: any) => {
       
        this.parent.showStatus ( "Image Changed" );

         var oldImageGuid: string = this.imageGuid ;

         // Update image on UI

         this.imageGuid = response ;
         
         // Delete existing image reference (ImageManager and Server won't delete unknown Image)
         ImageManager.DeleteImage ( oldImageGuid ) ;

         this.parent.forceUiUpdate();
    }
  }
}
