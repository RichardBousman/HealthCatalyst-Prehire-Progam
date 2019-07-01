using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleSearchServer.Models
{
    public class Image
    {
        /// <summary>
        /// GUID for the image when user hasn't provided an image
        /// </summary>
        public static readonly string UnknownImageGuid = "0";


        /// <summary>
        /// Update the number of people that are using the image, and if 0, delete the image
        /// </summary>
        /// <param name="guid">guid of the number of people accessing the image</param>
        /// <param name="delta">Change to add to the count of number of people using image</param>
        /// <param name="context">Existing context to use.  If null (default) then a context will be created</param>
        /// <param name="request">Http Request being processed at this moment</param>
        /// <returns>New number of people using the image</returns>
        public static async Task<int> UpdateImageCount ( string guid, int delta, HttpRequest request = null, ImageContext context = null )
        {
            if ( guid == UnknownImageGuid )
            {
                return 0;
            }

            if ( context == null )
            {
                using ( context = new ImageContext () )
                {
                    return await DoUpdateImageCount ( guid, delta, request, context );
                }
            }
            else
            {
                return await DoUpdateImageCount ( guid, delta, request, context );
            }
        }

        /// <summary>
        /// Update the number of people that are using the image, and if 0, delete the image
        /// </summary>
        /// <param name="guid">guid of the number of people accessing the image</param>
        /// <param name="delta">Change to add to the count of number of people using image</param>
        /// <returns>New number of people using the image</returns>
        private static async Task<int> DoUpdateImageCount ( string guid, int delta, HttpRequest request, ImageContext context )
        {
            int newCount = 0;

            try
            {
                Image originalImage = context.Image.First ( image => image.Id == guid );

                originalImage.NumberPeopleAssignedTo += delta;

                newCount = originalImage.NumberPeopleAssignedTo;

                if ( newCount <= 0 )
                {
                    originalImage.NumberPeopleAssignedTo = 0;
                    context.Image.Remove ( originalImage );
                    newCount = 0;
                }
                else
                {
                    context.Image.Update ( originalImage );
                }
                await context.SaveChangesAsync ();
            }
            catch ( Exception exception )
            {
                // Log Exception in appropriate place

                MessageHandler.Error ( "Trying to update the number of people assigned to an image", request, exception );
            }

            return newCount;
        }

        /// <summary>
        /// Primary Key
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Actual JPeg file contents
        /// </summary>
        public byte[] Jpeg
        {
            get;
            set;
        }

        /// <summary>
        /// Number of people that are using this image
        /// </summary>
        public int NumberPeopleAssignedTo
        {
            get;
            set;
        }

        /// <summary>
        /// Default Image Constructor
        /// </summary>
        public Image ()
        {
            Id = Guid.NewGuid ().ToString ();
            Jpeg = new byte[0];
            NumberPeopleAssignedTo = 0;
        }

        /// <summary>
        /// Create an Image from the bytes passed in
        /// </summary>
        /// <param name="jpegFile">Bytes from a JPeg file</param>
        public Image ( byte[] jpegFile ) : this ()
        {
            Jpeg = jpegFile;
        }

        /// <summary>
        /// Create an image from the jpeg file
        /// </summary>
        /// <param name="fileName">Name of file on the disk</param>
        public Image ( string fileName ) : this ()
        {
            if ( File.Exists ( fileName ) )
            {
                using ( BinaryReader jpegFile = new BinaryReader ( File.Open ( fileName, FileMode.Open ) ) )
                {
                    int fileLength = (int) jpegFile.BaseStream.Length;
                    Jpeg = new byte[fileLength];

                    jpegFile.BaseStream.Position = 0;
                    jpegFile.Read ( Jpeg, 0, fileLength );
                }
            }
        }

        /// <summary>
        /// Override of ToString to aide in debugging Images
        /// </summary>
        /// <returns>String describing the image object</returns>
        public override string ToString ()
        {
            return $"Image {Id}, assigned to {NumberPeopleAssignedTo} people.";
        }
    }
}
