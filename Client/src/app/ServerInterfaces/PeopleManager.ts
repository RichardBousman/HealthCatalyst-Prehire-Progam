/**
 * Manage the access to the Server for People (non-Image) data.
 */

import { sendRequest } from "selenium-webdriver/http";
import { Person } from "../model/person";


/**
 * Manage the accesses to the Server for People
 */
export abstract class PeopleManager {
    /** URL of the People Server */
//    public static readonly PeoplenterfaceUrl:string = 'https://localhost:44362/api/People';
    public static readonly PeoplenterfaceUrl:string = 'https://rbbhealthcatalystprehireserver.azurewebsites.net/api/People';

    /**
     * Get the people entries from the database.
     * 
     * TODO: Performance enhancement would be to return a 'small' group
     * at a time so that the first set can be displayed on the UI while the
     * remainder are being accessed
     * @param callback Called with the array of people returned from the server
     */
    public static GetPeople ( callback: ( receivedData: Array<Person> ) => void ) 
    {
        PeopleManager.sendRequest ( "GET", PeopleManager.PeoplenterfaceUrl, (receivedText) => {
            var peopleArray: Array<Person> = <Array<Person>> JSON.parse ( receivedText );
            callback ( peopleArray ) ;
        } ) ;
    }

    /**
     * Add a new person to the database
     * @param parameters set of parameters in the form of field=value+field=value+...
     * @param callback Function called with the newly created person passed in.
     */
    public static AddPerson ( parameters: string, callback: (newPerson: Person) => void) {

        PeopleManager.DoAddOrUpdate ( "POST", `${PeopleManager.PeoplenterfaceUrl}?changes=${parameters}`, callback ) ;
    }

   /**
     * Update a person to the database
     * @param parameters set of parameters in the form of field=value+field=value+... that are to be changed
     * @param callback Function called with the newly updated person passed in.
     */ 
    public static UpdatePerson ( personId: string, changes: string, callback: (newPerson: Person) => void) {

        PeopleManager.DoAddOrUpdate ( "PUT", `${PeopleManager.PeoplenterfaceUrl}?personId=${personId}&changes=${changes}`, callback ) ;
    }

    /**
     * Delete the person from the server's database, along with the image if it has been defined
     * @param personId Identity of the person to delete.
     */
    public static DeletePerson ( personId: string ) {
        PeopleManager.sendRequest ( "DELETE", `${PeopleManager.PeoplenterfaceUrl}/${personId}`, null ) ;
    }

    /**
     * Do the actual Add or Update (implemented to reuse code)
     * @param method "POST" or "PUT" for Add or Update respectively
     * @param url Actual url with parameters to pass to the post or put
     * @param callback Function that will be called with the added or updated Person entry in it
     */
    private static DoAddOrUpdate ( method: string, url: string, callback: (newPerson: Person) => void) {
        PeopleManager.sendRequest ( method, url, (receivedText) => {
            var updatedObject:Person = JSON.parse(receivedText) as Person ;
            callback ( updatedObject ) ;
        })
    }

    /**
     * Do the actual Send of the message to the People Server
     * @param method Method to send (GET, PUT, POST or DELETE)
     * @param url Full url (with parameters) to call
     * @param callback Callback that will get the response text passed to it
     */
    private static sendRequest( method: string, url: string, callback: ( receivedData: string ) => void ) {
        var request:XMLHttpRequest = new XMLHttpRequest();

        request.onreadystatechange = () => {
        if ( request.readyState == XMLHttpRequest.DONE ) {
            if ( request.status == 200 && 
                request.responseText != "undefined" ) {
                    if ( callback != null )
                    {
                        callback ( request.responseText ) ;
                    }
            }
        }
        };

        request.open(method, url );

        request.setRequestHeader ( "Content-Type", "application/json, charset=utf-8");

        // Send the request. The response is handled in the 
        // onCompletion function.
        request.send();
    } 
}
