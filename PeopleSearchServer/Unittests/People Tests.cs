using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeopleSearchServer.Controllers;
using PeopleSearchServer.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Unittests
{
    /// <summary>
    /// Test the People Class, the People Controller and the People Database Context (indirectly)
    /// </summary>
    [TestClass]
    public class PeopleTests
    {
        /// <summary>
        /// Create Context for InMemory database
        /// </summary>
        private static DbContextOptions<PeopleSearchContext> PeopleDbOptions
        {
            get
            {
                DbContextOptions<PeopleSearchContext> returnValue = new DbContextOptionsBuilder<PeopleSearchContext> ()
                    .UseInMemoryDatabase (databaseName: "People")
                    .Options;

                return returnValue;
            }
        }

        /// <summary>
        /// After each test, Delete all entries from database Interest and Person
        /// </summary>
        [TestCleanup]
        public void Cleanup ()
        {
            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                var itemsToDelete = context.Set<Interest>();
                context.Interest.RemoveRange ( itemsToDelete );

                var peopleToDelete = context.Set<Person>();
                context.Person.RemoveRange ( peopleToDelete );

                context.SaveChanges ();
            }
        }

        #region Test People Object

        /// <summary>
        /// Test Creation of Person functions
        /// </summary>
        [TestMethod]
        public void CreatePersonTests ()
        {
            // Test default Constructor (used by other constructors)
            Person emptyPerson = new Person ();

            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.FirstName ), "First Name is empty" );
            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.LastName ), "Last Name is empty" );
            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.AddressLine1 ), "Address Line 1 is empty" );
            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.AddressLine2 ), "Address Line 2 is empty" );
            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.City ), "Cityis empty" );
            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.StateOrTerritory ), "State is empty" );
            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.Country ), "Country is empty" );
            Assert.IsTrue ( string.IsNullOrEmpty ( emptyPerson.ZipCode ), "Zip code is empty" );
        }

        /// <summary>
        /// Test the Number of people using image functions
        /// </summary>
        [TestMethod]
        public void AddressTests ()
        {
            Person fredFlinestone = new Person
            {
                FirstName = "Fred",
                LastName = "Flinestone",
                AddressLine1 = "Third Rock on Right",
                AddressLine2 = "123 Rock Blvd.",
                City = "Bedrock",
                StateOrTerritory = "Some State",
                Country = "Stonehenge",
                ZipCode = "00001"
            };

            Assert.AreEqual ( "Third Rock on Right\r\n123 Rock Blvd.\r\nBedrock, Some State\r\n00001\r\nStonehenge",
                fredFlinestone.AddressAsText, "Address formatted as Text" );

            Assert.AreEqual ( "Third Rock on Right, 123 Rock Blvd., Bedrock, Some State, 00001, Stonehenge",
                fredFlinestone.AddressCommaSeparated, "Address line comma separated" );
        }

        #endregion Test People Object

        #region Test People Controller and People Context Objects

        /// <summary>
        /// Test the People Controller
        /// </summary>
        [TestMethod]
        public void PeopleControllerGetTests ()
        {
            // Test GET

            ActionResult<Person[]> results = DoHttpGet () ;

            Assert.AreEqual ( 0, results.Value.Length, "Empty Array returned" );

            AddPersonToDb ( "Richard", "Bousman" );

            results = DoHttpGet ();

            Assert.AreEqual ( 1, results.Value.Length, "One person returned" );
            Assert.IsTrue ( PersonExistsInDb ( "Richard", "Bousman" ), "Entry is in database" );
            Assert.IsTrue ( PersonExistsInArray ( results.Value, "Richard", "Bousman" ), "Person is in results" );

            Person fred = AddPersonToDb ( "Fred", "Flinestone" );

            results = DoHttpGet ();

            Assert.AreEqual ( 2, results.Value.Length, "Two people returned" );
            Assert.IsTrue ( PersonExistsInArray ( results.Value, "Richard", "Bousman" ), "Person is in results" );
            Assert.IsTrue ( PersonExistsInArray ( results.Value, "Fred", "Flinestone" ), "Fred is in results" );

            AddPersonToDb ( "Barney", "Rubble" );

            results = DoHttpGet ();

            Assert.AreEqual ( 3, results.Value.Length, "Should be three people in database now, Richard, Fred and Barney" );
            Assert.IsTrue ( PersonExistsInArray ( results.Value, "Richard", "Bousman" ), "Richard is in results" );
            Assert.IsTrue ( PersonExistsInArray ( results.Value, "Fred", "Flinestone" ), "Fred is in results" );
            Assert.IsTrue ( PersonExistsInArray ( results.Value, "Barney", "Rubble" ), "Barney is in results" );

            // Test get of a specific person

            ActionResult<Person> retrievedPerson = DoHttpGetPerson ( fred.PersonId );

            Assert.AreEqual ( fred.PersonId, retrievedPerson.Value.PersonId, "Id is the same" );
            Assert.AreEqual ( fred.FirstName, retrievedPerson.Value.FirstName, "First names are the same" );
            Assert.AreEqual ( fred.LastName, retrievedPerson.Value.LastName, "Last names are the same" );
        }

        /// <summary>
        /// Test the Post to the database works
        /// </summary>
        [TestMethod]
        public void PeoplePostTest ()
        {
            Assert.AreEqual ( 0, DoHttpGet ().Value.Length, "Nobody in database" );
            Assert.IsFalse ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred shouldn't be there...yet" );

            DoHttpPost ( "firstName=Fred@lastName=Flinestone" );

            Assert.IsTrue ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred is there now" );
            Assert.AreEqual ( 1, DoHttpGet ().Value.Length, "One person (Fred) in database" );
        }

        /// <summary>
        /// Test the Post to the database works
        /// </summary>
        [TestMethod]
        public void PeoplePutTest ()
        {
            Assert.IsFalse ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred shouldn't be there...yet" );

            DoHttpPost ( "firstName=Fred@lastName=Flinestone" );

            Assert.IsTrue ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred is there now" );

            Person wasFred = GetPersonFromDb ( "Fred", "Flinestone" );
            Assert.IsNotNull ( wasFred, "Fred was found in the database" );

            Person wilma = DoHttpPut ( wasFred.PersonId, "firstName=Wilma" );

            Assert.AreEqual ( 1, NumberEntriesInPersonDB (), "Only one entry in database." );
            Assert.IsTrue ( PersonExistsInDb ( "Wilma", "Flinestone" ), "The person in the database is Wilma" );
            Assert.AreEqual ( wasFred.PersonId, GetPersonFromDb ( "Wilma", "Flinestone" ).PersonId, "Person Id of Wilma is same as the old ID of Fred" );
        }

        /// <summary>
        /// Test deleting people from database
        /// </summary>
        [TestMethod]
        public void PeopleDeleteTest ()
        {
            Person richard = AddPersonToDb ( "Richard", "Bousman" );
            Person fred = AddPersonToDb ( "Fred", "Flinestone" );
            Person wilma = AddPersonToDb ( "Wilma", "Flinestone" );

            Assert.AreEqual ( 3, NumberEntriesInPersonDB (), "Three people in database, Richard, Fred and Wilma" );
            Assert.IsTrue ( PersonExistsInDb ( "Richard", "Bousman" ), "Richard is in database" );
            Assert.IsTrue ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred is in database" );
            Assert.IsTrue ( PersonExistsInDb ( "Wilma", "Flinestone" ), "Wilma is in database" );

            // Delete from middle of DB
            Person deleteResults = DoHttpDelete ( fred.PersonId );

            Assert.AreEqual ( fred.PersonId, deleteResults.PersonId, "Return value is correct" );
            Assert.AreEqual ( 2, NumberEntriesInPersonDB (), "Two people in database, Richard, and Wilma" );
            Assert.IsTrue ( PersonExistsInDb ( "Richard", "Bousman" ), "Richard is in database" );
            Assert.IsFalse ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred is NOT in database" );
            Assert.IsTrue ( PersonExistsInDb ( "Wilma", "Flinestone" ), "Wilma is in database" );

            // Delete Last entry in DB
            deleteResults = DoHttpDelete ( wilma.PersonId );

            Assert.AreEqual ( wilma.PersonId, deleteResults.PersonId, "Return value is correct" );
            Assert.AreEqual ( 1, NumberEntriesInPersonDB (), "One people in database, Richard." );
            Assert.IsTrue ( PersonExistsInDb ( "Richard", "Bousman" ), "Richard is in database" );
            Assert.IsFalse ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred is NOT in database" );
            Assert.IsFalse ( PersonExistsInDb ( "Wilma", "Flinestone" ), "Wilma is NOT in database" );

            // Delete First entry in DB
            deleteResults = DoHttpDelete ( richard.PersonId );

            Assert.AreEqual ( richard.PersonId, deleteResults.PersonId, "Return value is correct" );
            Assert.AreEqual ( 0, NumberEntriesInPersonDB (), "Zero people in database." );
            Assert.IsFalse ( PersonExistsInDb ( "Richard", "Bousman" ), "Richard is NOT in database" );
            Assert.IsFalse ( PersonExistsInDb ( "Fred", "Flinestone" ), "Fred is NOT in database" );
            Assert.IsFalse ( PersonExistsInDb ( "Wilma", "Flinestone" ), "Wilma is NOT in database" );
        }

        #endregion Test People Controller and People Context Objects

        #region Test helper functions to reduce code and make more readable

        /// <summary>
        /// Add the person to the database
        /// </summary>
        /// <param name="firstname">new person's firstname</param>
        /// <param name="lastname">New person's last name</param>
        /// <returns>Newly created person</returns>
        private Person AddPersonToDb ( string firstname, string lastname )
        {
            Person baby = new Person { FirstName = firstname, LastName = lastname } ;

            // Populate Database with another entry
            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                context.Person.Add ( baby );
                context.SaveChanges ();
            }

            return baby;
        }

        /// <summary>
        /// Simulate HTTP Get operation using actual controller returning actual results
        /// </summary>
        /// <param name="guid">GUID of image to Get</param>
        /// <returns>FileContentsResult</returns>
        private ActionResult<Person[]> DoHttpGet ()
        {
            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                PeopleController controller = new PeopleController ( context );

                // TEST GET

                ActionResult<Person[]> results = null ;
                Task.Run ( async () => results = await controller.Get () ).Wait ();
                return results;
            }
        }

        /// <summary>
        /// Execute Http Call GetPerson... Get{id}
        /// </summary>
        /// <param name="personId">Person to get</param>
        /// <returns>Action Results with returned person</returns>
        private ActionResult<Person> DoHttpGetPerson ( int personId )
        {
            ActionResult<Person> retrievedPerson = null;

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                PeopleController controller = new PeopleController ( context );

                // TEST GET

                Task.Run ( async () => retrievedPerson = await controller.GetPerson ( personId ) ).Wait ();
            }

            return retrievedPerson;
        }

        /// <summary>
        /// Execute Http Call Post (Add Person)
        /// </summary>
        /// <param name="changes">Changes, which in this case are the fields to assign to the new person</param>
        private void DoHttpPost ( string changes )
        {
            HttpContext httpContext = new DefaultHttpContext ();
            httpContext.Request.QueryString = new QueryString ( $"?changes={changes}" );

            IActionResult actionResult = null;

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                PeopleController controller = new PeopleController ( context )
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = httpContext
                    }
                };

                Task.Run ( async () => actionResult = await controller.PostNewPerson () ).Wait ();
                context.SaveChanges ();
            }
        }

        /// <summary>
        /// Execute Http Call Put (Update Person)
        /// </summary>
        /// <param name="personId">Person Id to modify</param>
        /// <param name="changes">Changes to apply</param>
        /// <returns>Updated person object</returns>
        private Person DoHttpPut ( int personId, string changes )
        {
             IActionResult results = null;

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                PeopleController controller = new PeopleController ( context );

                // TEST GET

                Task.Run ( async () => results = await controller.Update ( personId.ToString (), changes ) ).Wait ();
            }

            return (results as OkObjectResult).Value as Person ;
        }


        /// <summary>
        /// Simulate an HTTP Delete Call using actual controller
        /// </summary>
        /// <param name="guid">GUID to get</param>
        private Person DoHttpDelete ( int personId )
        {
            ActionResult < Person > results = null;

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                PeopleController controller = new PeopleController ( context );
                Task.Run ( async () => results = await controller.DeletePerson ( personId ) ).Wait ();
                context.SaveChanges ();
            }

            return results.Value;
        }

        /// <summary>
        /// Run asynchronous query into in-memory database to get the number of entries
        /// </summary>
        /// <returns>Number of entries in database</returns>
        private int NumberEntriesInPersonDB ()
        {
            int count = -1;

            Task.Run ( async () =>
            {
                using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
                {
                    count = await context.Person.CountAsync ();
                }
            } ).Wait ();

            return count;
        }

        /// <summary>
        /// Run asynchronous query into in-memory database to get the number of entries
        /// </summary>
        /// <returns>Number of entries in database</returns>
        private int NumberEntriesInInterestDB ()
        {
            int count = -1;

            Task.Run ( async () =>
            {
                using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
                {
                    count = await context.Interest.CountAsync ();
                }
            } ).Wait ();

            return count;
        }

        /// <summary>
        /// Verify that the Person entry exists in the database
        /// </summary>
        /// <param name="firstName">First name of person to check for in the database</param>
        /// <param name="lastName">Last name of person to check for in the database</param>
        /// <returns>True if the entry exists in the database, false otherwise.</returns>
        private bool PersonExistsInDb ( string firstName, string lastName )
        {
            return GetPersonFromDb ( firstName, lastName ) != null;
        }

        /// <summary>
        /// Get the specific person from the database
        /// </summary>
        /// <param name="firstName">Person to get's first name</param>
        /// <param name="lastName">Person to get's last name</param>
        /// <returns>Person from database, or null if not found</returns>
        private Person GetPersonFromDb ( string firstName, string lastName )
        {
            Person foundEntry = null ;

            try
            {
                using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
                {
                    Task.Run ( async () => foundEntry = await context.Person.FirstAsync ( entry =>
                            entry.FirstName == firstName && entry.LastName == lastName ) ).Wait ();
                }
            }
            catch
            {
                foundEntry = null;
            }

            return foundEntry;
        }

        /// <summary>
        /// Verify that the person exists in the array
        /// </summary>
        /// <param name="array">Array to search for person</param>
        /// <param name="firstname">First name of person looking for</param>
        /// <param name="lastname">Last name of person looking for</param>
        /// <returns>True if person found, otherwise false</returns>
        private bool PersonExistsInArray ( Person[] array, string firstname, string lastname )
        {
            Person foundEntry = array.FirstOrDefault ( person =>
                {
                    return person.FirstName == firstname && person.LastName == lastname;
                });

            return foundEntry != null;
        }

        #endregion Test helper functions to reduce code and make more readable
    }
}
