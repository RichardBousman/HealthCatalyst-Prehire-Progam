using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeopleSearchServer.Controllers;
using PeopleSearchServer.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Unittests
{
    /// <summary>
    /// Test the Image Class, the Image Controller and the Image Database Context (indirectly)
    /// </summary>
    [TestClass]
    public class SeedDataTests
    {
        /// <summary>
        /// Create Context for InMemory database for Image
        /// </summary>
        private static DbContextOptions<ImageContext> ImageDbOptions
        {
            get
            {
                DbContextOptions<ImageContext> returnValue = new DbContextOptionsBuilder<ImageContext> ()
                            .UseInMemoryDatabase ( databaseName: "image" )
                            .Options;

                return returnValue;
            }
        }

        /// <summary>
        /// Create Context for InMemory database for People
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
        /// File name of image of Unknown Person (default Image)
        /// </summary>
        private string UnknownImageFile
        {
            get
            {
                return @".\TestData\Unknown.jpg";
            }
        }

        /// <summary>
        /// Size of the contents of the Unknown file on the disk
        /// </summary>
        private long UnknownFileSize
        {
            get
            {
                FileInfo fileInfo = new FileInfo ( UnknownImageFile );

                return fileInfo.Length;
            }
        }

        /// <summary>
        /// After each test, Delete all entries from database Image
        /// </summary>
        [TestCleanup]
        public void Cleanup ()
        {
            using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
            {
                var imagesToDelete = context.Set<Image>();
                context.Image.RemoveRange ( imagesToDelete );
                context.SaveChanges ();
            }

            using ( PeopleSearchContext context = new PeopleSearchContext ( PeopleDbOptions ) )
            {
                var interestsToDelete = context.Set<Interest>();
                context.Interest.RemoveRange ( interestsToDelete );

                var peopleToDelete = context.Set<Person> ();
                context.Person.RemoveRange ( peopleToDelete );

                context.SaveChanges ();
            }
        }

        /// <summary>
        /// Test Creation of Seed Data
        /// </summary>
        [TestMethod]
        [DeploymentItem ( @".\SeedData\Unknown.jpg" )]
        [DeploymentItem ( @".\SeedData\Richard.jpg" )]
        [DeploymentItem ( @".\SeedData\Trent.jpg" )]
        public void TestSeedData ()
        {
            SeedData.Initialize ( ImageDbOptions, PeopleDbOptions );

            int numberImages = CountImagesInDB ();

            Assert.AreEqual ( 3, CountImagesInDB (), "Should be three images in database" );
            Assert.AreEqual ( 3, CountPeopleInDB (), "Should be three people in database" );
            Assert.AreEqual ( 5, CountInterestsInDB (), "Should be five interests in database" );
        }


        /// <summary>
        /// Run asynchronous query into in-memory database to get the number of Images
        /// </summary>
        /// <returns>Number of entries in database</returns>
        private int CountImagesInDB ()
        {
            int count = -1;

            Task.Run ( async () =>
            {
                using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
                {
                    count = await context.Image.CountAsync ();
                }
            } ).Wait ();

            return count;
        }


        /// <summary>
        /// Run asynchronous query into in-memory database to get the number of People
        /// </summary>
        /// <returns>Number of entries in database</returns>
        private int CountPeopleInDB ()
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
        /// Run asynchronous query into in-memory database to get the number of Interests
        /// </summary>
        /// <returns>Number of entries in database</returns>
        private int CountInterestsInDB ()
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
    }
}
