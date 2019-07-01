using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeopleSearchServer.Models;
using System;
using System.Linq;

namespace PeopleSearchServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors ()]
    public class imageController : ControllerBase
    {
        /// <summary>
        /// Minimum GUID Length used to test for invalid or Unknown GUID's
        /// </summary>
        private readonly byte MinimumGuidLength = 32;

        private readonly ImageContext _context;

        /// <summary>
        /// Cache holding the Unknown Image
        /// </summary>
        private Image UnknownImage
        {
            get
            {
                if ( unknownImageBackingVariable == null )
                {
                    unknownImageBackingVariable = _context.Image.First ( image => image.Id == Image.UnknownImageGuid );
                }

                return unknownImageBackingVariable;
            }
        }
        private Image unknownImageBackingVariable = null;

        /// <summary>
        /// Constructor of Image Controller.  Dependency injection provides ImageContext value.
        /// </summary>
        /// <param name="context"></param>
        public imageController ( ImageContext context )
        {
            _context = context;
        }

        // GET: api/upload/5
        [HttpGet]
        public ActionResult Get([FromQuery(Name = "guid")] string guid)
        {
            Image returnValue;

            if ( useUnknownGuid (guid) )
            {
                returnValue = UnknownImage;
           }
            else
            {
                try
                {
                    returnValue = _context.Image.First ( image => image.Id == guid );
                }
                catch
                {
                    returnValue = UnknownImage;
                }
            }

            return File ( returnValue.Jpeg, "image/jpeg" );
            //return File ( returnValue.Jpeg, "application/octet-stream" );
        }

        [HttpPost]
        public string Post ()
        {
            foreach ( IFormFile file in Request.Form.Files )
            {
                if ( file == null || file.Length == 0 )
                {
                    break;
                }

                byte[] fileContents = new byte[file.Length];

                // handle file here
                var stream = file.OpenReadStream();
                {
                    stream.Read ( fileContents, 0, (int) file.Length );
                }
                stream.Position = 0;
                stream.Close ();

                Image newImage = new Image ( fileContents );

                try
                {
                    var actualImageEnty = _context.Image.Add ( newImage );
                    _context.SaveChanges ();

                    return newImage.Id;
                }
                catch ( Exception exception )
                {
                    // Log Error
                    System.Diagnostics.Debug.WriteLine ( $"Error creating image: '{exception.Message}'" );
                 }
            }

            return Image.UnknownImageGuid;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public void Delete( [FromQuery ( Name = "guid" )] string guid )
        {
            if ( useUnknownGuid (guid) )
            {
                return; // Don't delete the Unknown Image which is used when no image available.
            }

            Image.UpdateImageCount ( guid, -1, this.Request, _context ).Wait() ;
        }

        private bool useUnknownGuid ( string guid )
        {
            return guid == null ||
                    guid == Image.UnknownImageGuid ||
                    guid.Length < MinimumGuidLength ;
        }

    }
}
