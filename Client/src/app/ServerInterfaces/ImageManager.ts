import { ConfigurationSettings } from "../Configuration";

/**
 * Manage the accesses to the Image Server.
 * 
 * Note: Adding of the image to the server (POST) is done automatically by the FileUploader utility
 */
export abstract class ImageManager {
    /** URL of the Image Interface (Server) */
    //public static readonly ImageInterfaceUrl:string = 'https://localhost:44362/api/image';
    public static readonly ImageInterfaceUrlold:string = 'https://rbbhealthcatalystprehireserver.azurewebsites.net/api/image';
    public static readonly ImageInterfaceUrl:string = `${ConfigurationSettings.ServerRoot}/api/image` ;

    /** [Invalid] GUID of the image for the image used when no image available. */
    public static readonly UnknownImageGuid = '0';

    /**
     * Get the Image URL that can be used to download the actual image from the server
     * @param guid GUID of the image to retrieve
     */
    public static GetImageUrl ( guid: string ) {
        return this.ImageInterfaceUrl + '?guid=' + guid ;
    }

    /**
     * Delete the image from the server (or at least decrement the number of people using the image)
     * @param guid GUID of the image to decrement the count or delete
     */
    public static DeleteImage ( guid: string ) {

        if ( guid != this.UnknownImageGuid )
        {
            this.sendRequest ( 'DELETE', guid, null );
        }
    }
   
    /**
     * Send the actual request to the server
     * @param method Method (GET, PUT, POST, DELETE) to send request to
     * @param guid GUID of the image in question
     * @param callback Callback to send the response to
     */
    private static sendRequest(method: string, guid: string, callback: (receivedData: string) => void) {
        var request:XMLHttpRequest = new XMLHttpRequest();

        request.onreadystatechange = () => {
            if ( request.readyState == XMLHttpRequest.DONE ) {
                if ( request.responseText != 'undefined' ) {
                    if ( callback != null )
                    {
                        callback ( request.response );
                    }
                }
            }
        };

       request.open(method, `${ImageManager.ImageInterfaceUrl}/?guid=${guid}`);

        // Send the request. The response is handled in the 
        // onCompletion function.
        request.send();
    }
}
