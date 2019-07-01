/**
 * Manage a single interest node during add/removal of interests.
 */

export enum InterestState {
    OriginalInterest,
    DeletedOriginal,
    Added,
    DeletedAdded
}


export class InterestUpdateNode {
    interest: string;
    state: InterestState ;

    /**
     * Create a new Interest.  This manages sending deletes to the server of only interests that already exist on the server
     * @param interestName The Interest
     * @param howCreated Whether the interest was created 'just now' or it exists on the server.
     */
    static NewInterestUpdate ( interestName: string, howCreated: InterestState = InterestState.OriginalInterest ) : InterestUpdateNode
    {
        let returnValue = new InterestUpdateNode() ;
        returnValue.Initialize ( interestName, howCreated );
        return returnValue ;
    }

    /**
     * Only allow creation of interest nodes via the NewInterestUpdate function
     */
    private constructor ()
    {
    }

    /**
     * Initialize the Interest
     * @param interestName Interest Name
     * @param howCreated How Created (i.e. Original Interests are those on the server that originally existed)
     */
    private Initialize ( interestName: string, howCreated: InterestState = InterestState.OriginalInterest)
    {
        this.interest = interestName ;
        this.state = howCreated ;
    }

    /**
     * Flag the interest as deleted.
     */
    public SetDeleted () {
        if ( this.state == InterestState.OriginalInterest )
        {
            this.state = InterestState.DeletedOriginal ;
        }
        else if ( this.state == InterestState.Added )
        {
            this.state = InterestState.DeletedAdded ;
        }
    }
}