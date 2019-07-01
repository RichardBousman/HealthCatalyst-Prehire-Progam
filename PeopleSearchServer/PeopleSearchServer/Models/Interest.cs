using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleSearchServer.Models
{

    public class Interest
    {
        /// <summary>
        /// Process the changes to the Interest database as specified in the change requests
        /// </summary>
        /// <param name="person">Person to associate the interest with</param>
        /// <param name="changeRequests">Set of changes of the form 'verb:interest' where verb is one of 'Add' or 'Del' for
        /// add or delete repectively, and the interest is the interest to add or delete.</param>
        /// <returns>Number of entries in the change list that were processed.</returns>
        public static async Task<int> ProcessChanges ( Person person, List<string> changeRequests, PeopleSearchContext context )
        {
            int numberChanged = 0;

            Person dbPerson = context.Person.Where ( entry => entry.PersonId == person.PersonId ).Single();

            foreach ( string change in changeRequests )
            {
                string[] parts = change.Split (":");

                if ( parts.Length == 2 )
                {
                    string verb = parts[0];
                    string interest = parts[1];

                    switch ( verb )
                    {
                        case "Add":
                            context.Interest.Add ( new Interest
                            {
                                Person = dbPerson,
                                TheInterest = interest
                            } );
                            numberChanged++;
                            break;

                        case "Del":

                            context.Interest.RemoveRange (
                                context.Interest.Where ( entry => entry.Person == dbPerson &&
                                        entry.TheInterest.ToLower() == interest.ToLower() ) );
                            numberChanged++;
                            break;

                        default:

                            MessageHandler.Error ( $"Invalid verb ({verb}) in Interest change list" );
                            break;
                    }
                }
            } // FOREACH entry in the change requests list

            await context.SaveChangesAsync ();

            return numberChanged;
        }

        /// <summary>
        /// Unique identifier (Key) for database
        /// </summary>
        public int InterestId
        {
            get;
            set;
        }

        /// <summary>
        /// Person who has this interest
        /// </summary>
        public Person Person
        {
            get;
            set;
        }

        /// <summary>
        /// Person's Interest
        /// </summary>
        [Display(Name = "Interest")]
        public string TheInterest
        {
            get;
            set;
        }
    }
}
