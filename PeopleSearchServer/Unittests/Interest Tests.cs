using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeopleSearchServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unittests
{
    /// <summary>
    /// Test the Interests Class
    /// </summary>
    [TestClass]
    public class InterestTests
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

                var personToDelete = context.Set<Person>();
                context.Person.RemoveRange ( personToDelete );

                context.SaveChanges ();
            }
        }

        /// <summary>
        /// Test Processing Creating Interests
        /// </summary>
        [TestMethod]
        public void AddInterestTests ()
        {
            Person person = CreatePersonAndAddToDatabase ( "Fred", "Flinestone" );

            List<string> changes = new List<string>()
            {
                "Add:Programming",
                "Add:Sleeping"
            };

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                Interest.ProcessChanges ( person, changes, context ).Wait();
            }

            Assert.AreEqual ( changes.Count, NumberEntriesInDB (), "All entries are Adds so should have created as many entries as in list" );

            Assert.IsTrue ( EntryExists ( person, "Programming" ), "Entry must be in database" );
            Assert.IsTrue ( EntryExists ( person, "Sleeping" ), "Entry must be in database" );
        }

        /// <summary>
        /// Test adding two entries, then later deleting one of them
        /// </summary>
        [TestMethod]
        public void TestAddAndDeleteSeparate ()
        {
            Person person = CreatePersonAndAddToDatabase ( "Fred", "Flinestone" );

            List<string> changes = new List<string>()
            {
                "Add:Programming",
                "Add:Sleeping"
            };

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                Interest.ProcessChanges ( person, changes, context ).Wait ();
            }

            Assert.AreEqual ( 2, NumberEntriesInDB (), "All entries are Adds so should have created as many entries as in list" );

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                Interest.ProcessChanges ( person, new List<string> { "Del:Sleeping" }, context ).Wait ();
            }

            Assert.AreEqual ( 1, NumberEntriesInDB (), "Only one entry left in list after deleting one" );
            Assert.IsTrue ( EntryExists ( person, "Programming" ), "Entry must be in database" );
        }

        /// <summary>
        /// Test adding two entries first, then adding a third entry and deleting one of the previously
        /// added entries within a single call to ProcessChanges.
        /// </summary>
        [TestMethod]
        public void TestAddintAndDeleteTogether ()
        {
            Person person = CreatePersonAndAddToDatabase ( "Fred", "Flinestone" );

            List<string> changes = new List<string>()
            {
                "Add:Programming",
                "Add:Sleeping"
            };

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                Interest.ProcessChanges ( person, changes, context ).Wait ();
            }

            Assert.AreEqual ( 2, NumberEntriesInDB (), "All entries are Adds so should have created as many entries as in list" );

            Assert.IsTrue ( EntryExists ( person, "Programming" ), "Entry must be in database" );
            Assert.IsTrue ( EntryExists ( person, "Sleeping" ), "Entry must be in database" );

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                Interest.ProcessChanges ( person, new List<string> { "Add:Eating", "Del:Sleeping" }, context ).Wait ();
            }

            Assert.AreEqual ( 2, NumberEntriesInDB (), "Still two entries after adding one and deleting a different one" );

            Assert.IsTrue ( EntryExists ( person, "Programming" ), "Entry must be in database" );
            Assert.IsTrue ( EntryExists ( person, "Eating" ), "Entry must be in database" );
        }

        /// <summary>
        /// Create the person and add them to the database
        /// </summary>
        /// <param name="firstName">New person's first name</param>
        /// <param name="lastName">New Person's last name</param>
        /// <returns>New person object</returns>
        private Person CreatePersonAndAddToDatabase ( string firstName, string lastName )
        {
            Person results = new Person { FirstName = firstName, LastName = lastName } ;

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                context.Person.Add ( results );
                context.SaveChanges ();
            }

            return results;
        }

        /// <summary>
        /// Run asynchronous query into in-memory database to get the number of entries
        /// </summary>
        /// <returns>Number of entries in database</returns>
        private int NumberEntriesInDB ()
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
        /// Verify that the Interest entry exists in the database
        /// </summary>
        /// <param name="person">Person who owns the interest in the database</param>
        /// <param name="interest">Interest that we are looking for in the database</param>
        /// <returns>True if the entry exists in the database, false otherwise.</returns>
        private bool EntryExists ( Person person, string interest )
        {
            Interest foundEntry = null ;

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                Task.Run ( async () =>  foundEntry = await context.Interest.FirstAsync (entry => entry.Person == person && entry.TheInterest == interest ) ).Wait();
            }

            return foundEntry != null ;
        }
    }
}
