using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PeopleSearchServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PeopleSearchServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors()]
    public class PeopleController : ControllerBase
    {
        private readonly PeopleSearchContext _context;

        /// <summary>
        /// Constructor for PeopleSearchServer
        /// </summary>
        /// <param name="context"></param>
        public PeopleController(PeopleSearchContext context)
        {
            _context = context;
        }

        // GET: api/People
        [HttpGet]
        public async Task<ActionResult<Person[]>> Get()
        {
            Person[] results = await _context.Person.ToArrayAsync();

            return results;
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            var person = await _context.Person.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            return person;
        }

        /// <summary>
        /// Post a new Person to the database
        /// </summary>
        /// <returns>HTTP Status, with OK also returns the new person</returns>
        [HttpPost]
        public async Task<IActionResult> PostNewPerson ()
        {
            string changes = HttpUtility.UrlDecode (this.Request.QueryString.Value.Substring (1));

            changes = changes.RemoveLeading ( "changes=" );

            Person baby = new Person();

            bool anyData = ApplyChanges(changes.Split("@"), ref baby, out List<string> interestChanges );

            if ( anyData )
            {
                var newPerson = _context.Person.Add(baby);
                await _context.SaveChangesAsync ();

                await Image.UpdateImageCount ( baby.ImageGUID, 1, this.Request );

                await Interest.ProcessChanges ( baby, interestChanges, _context );

                return Ok ( baby );
            }

            return BadRequest ();
        }

        /// <summary>
        /// Update the person with the changes
        /// </summary>
        /// <param name="personId">Person to update</param>
        /// <param name="changes">Changes to apply</param>
        /// <returns>HTTP Status, with OK the updated person is returned</returns>
        [HttpPut]
        public async Task<IActionResult> Update( string personId, string changes)
        {
            if (ModelState.IsValid)
            {
                if (int.TryParse(personId, out int id))
                {
                    Person person = await _context.Person.FirstOrDefaultAsync(entry => entry.PersonId == id);

                    string oldImageGuid = person.ImageGUID;

                    if (person != null && ApplyChanges(changes.Split("@"), ref person, out List<string> interestChanges ))
                    {
                        _context.Person.Update(person);
                        await _context.SaveChangesAsync();

                        // IF the image was changed then update the image counts, possibly deleting the old image
                        if ( person.ImageGUID != oldImageGuid )
                        {
                            await Image.UpdateImageCount ( person.ImageGUID, 1, this.Request );
                            await Image.UpdateImageCount ( oldImageGuid, -1, this.Request );
                        }

                        await Interest.ProcessChanges ( person, interestChanges, _context );

                        return Ok ( person );
                    }
                }
            }

            return BadRequest();
        }

        /// <summary>
        /// Apply changes specified in set of updates to the record
        /// </summary>
        /// <param name="updates">Set of updates in form of field=value</param>
        /// <param name="record">Person record to update</param>
        /// <returns>True if there were any updates applied</returns>
        private bool ApplyChanges(string[] updates, ref Person record, out List<string> interestChanges )
        {
            interestChanges = new List<string> ();
            bool anyChangesApplied = false;

            foreach (string update in updates)
            {
                string[] changeRequest = update.Split('=');

                if (changeRequest.Length == 2)
                {
                    string field = changeRequest[0];
                    string newValue = changeRequest[1];

                    switch (field)
                    {
                        case "firstName":
                            record.FirstName = newValue;
                            anyChangesApplied = true;
                            break;

                        case "lastName":
                            record.LastName = newValue;
                            anyChangesApplied = true;
                            break;

                        case "addressLine1":
                            record.AddressLine1 = newValue;
                            anyChangesApplied = true;
                            break;

                        case "addressLine2":
                            record.AddressLine2 = newValue;
                            anyChangesApplied = true;
                            break;

                        case "city":
                            record.City = newValue;
                            anyChangesApplied = true;
                            break;

                        case "state":
                            record.StateOrTerritory = newValue;
                            anyChangesApplied = true;
                            break;

                        case "country":
                            record.Country = newValue;
                            anyChangesApplied = true;
                            break;

                        case "zip":
                            record.ZipCode = newValue;
                            anyChangesApplied = true;
                            break;

                        case "imageGuid":
                            record.ImageGUID = newValue;
                            anyChangesApplied = true;
                            break;

                        case "AddInterest":
                            interestChanges.Add ( $"Add:{newValue}" );
                            anyChangesApplied = true;
                            break;

                        case "DeleteInterest":
                            interestChanges.Add ( $"Del:{newValue}" );
                            anyChangesApplied = true;
                            break;
                    }
                }
            }

            return anyChangesApplied;
        }

        // DELETE: api/People/5
        [HttpDelete("{personId}")]
        public async Task<ActionResult<Person>> DeletePerson(int personId)
        {
            var person = await _context.Person.FindAsync(personId);
            if (person == null)
            {
                return NotFound();
            }

            _context.Interest.RemoveRange ( _context.Interest.Where ( interest => interest.Person == person ) );
            _context.Person.Remove(person);
            await _context.SaveChangesAsync();

            await Image.UpdateImageCount ( person.ImageGUID, -1, this.Request );

            return person;
        }

        /// <summary>
        /// Return true if the person ID exists in the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.PersonId == id);
        }
    }
}
