/**
 * Keep track of the interests that have been modified during this add/edit
 */

import { Person } from "../model/person";
import { InterestUpdateNode, InterestState } from "./InterestUpdateNode";

export class InterestsUpdates {
    owner: Person;
    interests: Array<InterestUpdateNode> ;
    deletedInterests: Array<InterestUpdateNode> ;

    /**
     * Create an empty set of interests
     */
    constructor ()
    {
        this.interests = new Array<InterestUpdateNode> () ;
        this.deletedInterests = new Array<InterestUpdateNode> () ;
    }

    /**
     * Initialize the set of interests that previously exist (prior to this edit)
     * @param person Person that has the interests
     */
    public Initialize ( person: Person ) {
 
        this.owner = person ;

        this.createOriginalList () ;
    }

    /**
     * The interest is not already on the set of interests identified
     * @param thingToLookFor Interest we looking for
     * @returns True if interest is not on list of current interests
     */
    public NotAlreadyAnInterest ( thingToLookFor: string ): boolean {
        return this.FindInterestNode ( thingToLookFor ) == undefined ;
    }

    /**
     * Add the interest to the set of interests for the user.
     * @param interest Interest to add
     * @returns True if interest added
     */
    public AddInterest ( interest: string ) : boolean {

          // Don't add the same interest twice
        if ( this.NotAlreadyAnInterest ( interest ) )
        {
            let deletedNode: InterestUpdateNode = this.FindDeletedNode ( interest ) ;

            if ( deletedNode != undefined )
            {
                // Node was already deleted.  Restore the deleted entry
                if ( deletedNode.state == InterestState.DeletedOriginal )
                {
                    // Restore node as if it was never deleted

                    deletedNode.state = InterestState.OriginalInterest ;
                 }
                else
                {
                    // Set node as an Added node

                    deletedNode.state = InterestState.Added ;
                }

                // Put node back on good list
                this.interests.push ( deletedNode ) ;
                this.RemoveNode ( this.deletedInterests, deletedNode ) ;
            }
            else
            {
                this.interests.push ( InterestUpdateNode.NewInterestUpdate ( interest, InterestState.Added ) ) ;
            }

            return true ;
        }
        else
        {
            return false ;
        }
    }

    /**
     * Delete the interest from set of user interests.  Keep track of deleted interests
     * that exist on the server, so that if re-added, we don't need to delete and add.
     * @param interest Interest to delete
     */
    public DeleteInterest ( interest: string ): void {

        let node: InterestUpdateNode = this.FindInterestNode ( interest ) ;

        if ( node != undefined )
        {
            // Keep track of any node that was deleted that is on the server
            // so that we can report back to the server to delete it
            if ( node.state == InterestState.OriginalInterest )
            {
                node.SetDeleted () ;
                this.deletedInterests.push ( node ) ;
            }

            // Always remove node from current interests
            this.RemoveNode ( this.interests, node ) ;
        }
    }

    /**
     * Create a set of changes to the interests to be passed to the server.
     * @returns Set of changes for the server to apply all at once.
     */
    public MakeInterestList () : string {
        let results: string = "" ;

        // Add the newly added interests to the results

        this.interests.forEach ( node => {
            if ( node.state == InterestState.Added )
            {
                if ( results.length > 0 )
                {
                    results = results + "@" ;
                }

                results = results + `AddInterest=${node.interest}` ;
            }
        });

        // Add the delete original nodes to the results

        this.deletedInterests.forEach(node => {
            if ( results.length > 0 )
            {
                results = results + "@" ;
            }

            results = results + `DeleteInterest=${node.interest}` ;
        });

        return results;
    }

    /**
     * Find interest on set of user interests
     * @param interestLookingFor Interest that we are interested in
     * @returns Interest node on the list of user interests, or undefined
     */
    private FindInterestNode ( interestLookingFor: string ) : InterestUpdateNode {
        return this.FindInArray ( this.interests, interestLookingFor ) ;
    }

    /**
     * Find the interest on the set of deleted interests.
     * @param interestLookingFor Interest we are interested in
     * @returns Interest node on the list of deleted interests, or undefined
     */
    private FindDeletedNode ( interestLookingFor: string ) : InterestUpdateNode {
        return this.FindInArray ( this.deletedInterests, interestLookingFor ) ;
    }

    /**
     * Find the interest in the set (Array) of interests
     * @param array Set of interests to serach for interest
     * @param interest Interest we are interested in.
     * @return Interest node in Array or undefined.
     */
    private FindInArray ( array: Array<InterestUpdateNode>, interest: string ): InterestUpdateNode {
        return array.find (  node => 
            node.interest.toLowerCase () == interest.toLowerCase() ) ;
    }

    /**
     * Remove the node from the set (array) of interest nodes
     * @param array Set of interest nodes to remove entry from
     * @param node Node to remove from list
     */
    private RemoveNode ( array: Array<InterestUpdateNode>, node: InterestUpdateNode ) {
    
        let index: number = array.indexOf ( node ) ;
        if ( index !== -1) array.splice (index, 1 ) ;
    }

    /**
     * Create the set of Interest Nodes that the user has at the start of this edit/add interaction
     */
    private createOriginalList () {
        this.owner.interests.forEach(interest => { 
            this.interests.push ( InterestUpdateNode.NewInterestUpdate ( interest )) ; 
        }, this );
    }
}