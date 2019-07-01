/**
 * Keep track of a single person in the Database.
 */

import { ImageManager } from "../ServerInterfaces/ImageManager";

export class Person {
    
  /**
   * Unique person ID field assigned by the database
   */
    personId: string;

    /**
     *  Person's address as HTML */
    addressAsHTML: string;

    /** Address as a multi-line text field */
    addressAsText: string;

    /** Address as a comma seperated text field */
    addressCommaSeparated: string;

    /** Person's first name (required) */
    firstName: string;

    /** Person's last name (required) */
    lastName: string;

    /** First line of Person's Address */
    addressLine1: string;

    /** Second line of Person's Address */
    addressLine2: string;

    /** City Person resides in */
    city: string;

    /** State or Territory Person resides in */
    stateOrTerritory: string;

    /** Country Peson resides in */
    country: string;

    /** Person's Zip code */
    zipCode: string;

    /** Person's Birthdate (as Date) */
    birthDate: Date;

    /** Person's Birthdate (as String) */
    displayBirthDate: string;

    /** Calculated age of person based on date of birth and todays date */
    age: string;

    /** Database GUID of the image to display for the person.  "0" means no Image */
    imageGUID: string;

    /** Is the person's entry on the UI highlighted */
    highlight: boolean;

    /** Are we showing the details for the person on the UI */
    showDetails: boolean;

    /** Are we editing the person on the UI */
    editDetails: boolean; 

    /** Set of interests (Array<string>) that the person has */
    interests: Array<string>;

    /** Create a new person with no interests */
    constructor (){
      this.interests = new Array<string>();
    }

    /** Returns the image of the Person to use on the UI */
    Photo ()
    {
      return ImageManager.GetImageUrl ( this.imageGUID ) ;
    }

    /**
     * Copy the values from one person to another
     * @param source Source of the values to copy to 'this'
     */
    CopyValues ( source:Person )
    {
        this.firstName = source.firstName ;
        this.lastName = source.lastName;
        this.addressLine1 = source.addressLine1 ;
        this.addressLine2 = source.addressLine2 ;
        this.addressAsHTML = source.addressAsHTML ;
        this.addressAsText = source.addressAsText;
        this.addressCommaSeparated = source.addressCommaSeparated ;
        this.city = source.city ;
        this.stateOrTerritory = source.stateOrTerritory ;
        this.country = source.country ;
        this.zipCode = source.zipCode ;
        this.birthDate = source.birthDate ;
        this.displayBirthDate = source.displayBirthDate ;
        this.age = source.age ;
      }
}