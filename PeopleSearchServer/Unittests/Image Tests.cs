using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeopleSearchServer.Controllers;
using PeopleSearchServer.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Unittests
{
    /// <summary>
    /// Test the Image Class, the Image Controller and the Image Database Context (indirectly)
    /// </summary>
    [TestClass]
    public class ImageTests
    {
        /// <summary>
        /// Create Context for InMemory database
        /// </summary>
        private static DbContextOptions<ImageContext> ImageDbOptions
        {
            get
            {
                DbContextOptions<ImageContext> returnValue = new DbContextOptionsBuilder<ImageContext> ()
                    .UseInMemoryDatabase (databaseName: "Image")
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
                var itemsToDelete = context.Set<Image>();
                context.Image.RemoveRange ( itemsToDelete );
                context.SaveChanges ();
            }
        }

        /// <summary>
        /// Test Creation of Image functions
        /// </summary>
        [TestMethod]
        [DeploymentItem ( @".\TestData\Unknown.jpg" )]
        public void CreateImageTests ()
        {
            // Test default Constructor (used by other constructors)
            Image emptyImage = new Image ();

            Assert.AreEqual ( 36, emptyImage.Id.Length, "Guid created" );
            Assert.AreEqual ( 0, emptyImage.Jpeg.Length, "Empty byte array" );

            // Test Constructor that takes in the jpeg file contents (used by Uploader from Angular)
            byte[] fakeJpegFile = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B };

            Image elevenByteImage = new Image ( fakeJpegFile );

            Assert.AreEqual ( 36, elevenByteImage.Id.Length, "Guid created" );
            Assert.AreEqual ( fakeJpegFile.Length, elevenByteImage.Jpeg.Length, "All data bytes stored in image" );

            // Test constructor that takes in a file name (used for seed data)
            Image unknownImage = new Image ( UnknownImageFile );

            Assert.AreEqual ( 36, unknownImage.Id.Length, "Guid created" );
            Assert.AreEqual ( UnknownFileSize, unknownImage.Jpeg.Length, "Internal size of file equals disk file size" );

            // Check GUIDs Unique
            Assert.AreNotEqual ( emptyImage.Id, elevenByteImage.Id, "GUIDs not the same" );
            Assert.AreNotEqual ( elevenByteImage.Id, unknownImage.Id, "GUIDs not the same" );
            Assert.AreNotEqual ( unknownImage.Id, emptyImage.Id, "GUIDs not the same" );
        }

        /// <summary>
        /// Test the Number of people using image functions
        /// </summary>
        [TestMethod]
        [DeploymentItem ( @".\TestData\Unknown.jpg" )]
        public void NumberPeopleUsingImageTests ()
        {
            Image image1 = new Image ( UnknownImageFile );
            Image image2 = new Image ( UnknownImageFile );
            Image image3 = new Image ( UnknownImageFile );

            string guid2 = image2.Id;

            // Populate Database
            using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
            {
                context.Image.AddRange ( image1, image2, image3 );
                context.SaveChanges ();
            }

            Assert.AreEqual ( 3, NumberEntriesInDB (), "Verify there are three entries in the database" );

            // Test Image number of people using image operations
            using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
            {
                Image image = context.Image.First ( dbImage => dbImage.Id == guid2 );

                Assert.AreEqual ( 0, image.NumberPeopleAssignedTo, "Initially noone assigned to the image" );

                Task.Run ( async () => await Image.UpdateImageCount ( guid2, 17, null, context ) ).Wait ();

                Task.Run ( async () => await Image.UpdateImageCount ( guid2, -7, null, context ) ).Wait ();

                Assert.AreEqual ( 10, image.NumberPeopleAssignedTo, "0 + 17 - 7 should equal 10 people assigned to the image" );
            }
        }

        /// <summary>
        /// Test the Image Controller
        /// </summary>
        [TestMethod]
        [DeploymentItem ( @".\TestData\Unknown.jpg" )]
        public void ImageControllerTests ()
        {
            Image image1 = new Image ( UnknownImageFile );
            Image image2 = new Image ( UnknownImageFile );
            Image image3 = new Image ( UnknownImageFile );

            string guid2 = image2.Id;

            // Populate Database
            using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
            {
                context.Image.AddRange ( image1, image2, image3 );
                context.SaveChanges ();
            }


            // Test GET

            FileContentResult actualResults = DoHttpGet ( image2.Id ) ;

            Assert.AreEqual ( UnknownFileSize, actualResults.FileContents.LongLength, "Results of Get from database same length as file on disk" );
            Assert.AreEqual ( "image/jpeg", actualResults.ContentType, "Correct Content Type" );

            Assert.IsTrue ( ArrayStartsWith ( actualResults.FileContents, 255, 216, 255, 224 ), "JPEG file starts with FF D8 FF E0" );


            // Test DELETE

            Assert.AreEqual ( 3, NumberEntriesInDB (), "Should be three entries in DB before DELETE" );

            DoHttpDelete ( image2.Id );
            Assert.AreEqual ( 2, NumberEntriesInDB (), "Only two entries in DB after first DELETE" );

            DoHttpDelete ( image3.Id );
            Assert.AreEqual ( 1, NumberEntriesInDB (), "One entry left in DB after second DELETE" );

            DoHttpDelete ( image1.Id );
            Assert.AreEqual ( 0, NumberEntriesInDB (), "Image db should now be empty" );
        }

        /// <summary>
        /// Simulate HTTP Get operation using actual controller returning actual results
        /// </summary>
        /// <param name="guid">GUID of image to Get</param>
        /// <returns>FileContentsResult</returns>
        private FileContentResult DoHttpGet ( string guid )
        {
            using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
            {
                imageController controller = new imageController ( context );

                // TEST GET

                ActionResult results = controller.Get ( guid );

                return results as FileContentResult;
            }
        }

        /// <summary>
        /// Simulate an HTTP Delete Call using actual controller
        /// </summary>
        /// <param name="guid">GUID to get</param>
        private void DoHttpDelete ( string guid )
        {
            using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
            {
                imageController controller = new imageController ( context );
                controller.Delete ( guid );
                context.SaveChanges ();
            }
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
                using ( ImageContext context = new ImageContext ( ImageDbOptions ) )
                {
                    count = await context.Image.CountAsync ();
                }
            } ).Wait ();

            return count;
        }

        /// <summary>
        /// Verify that the array passed in starts with the character set passed in
        /// </summary>
        /// <param name="array">Array of bytes to test</param>
        /// <param name="values">Values to verify are in array</param>
        /// <returns></returns>
        private bool ArrayStartsWith ( byte[] array, params byte[] values )
        {
            int index = 0;

            foreach ( byte singleByte in values )
            {
                if ( array[index] != singleByte )
                {
                    return false;
                }
                index++;
            }

            return true;
        }
    }
}
