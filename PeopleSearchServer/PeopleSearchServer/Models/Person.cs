using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PeopleSearchServer.Models
{
    /// <summary>
    /// Model for the Person Database Entry.  This describes a single person.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Unique identifier (Key) for database
        /// </summary>
        [Key]
        public int PersonId
        {
            get;
            set;
        }

        /// <summary>
        /// Person's First Name
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        public string FirstName
        {
            get;
            set;
        }

        /// <summary>
        /// Person's Last Name
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        public string LastName
        {
            get;
            set;
        }

        /// <summary>
        /// Date of Birth
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate
        {
            get;
            set;
        }

        [NotMapped]
        public int Age
        {
            get
            {
                TimeSpan howLongAlive = DateTime.Now - BirthDate ;

                return (int) Math.Floor ( howLongAlive.TotalDays / 365.0);
            }
        }

        [NotMapped]
        public string DisplayBirthDate
        {
            get
            {
                return BirthDate.ToString("MM/dd/yyyy");
            }
        }

        /// <summary>
        /// GUID of Image of person
        /// </summary>
        [Display(Name = "Image GUID")]
        public string ImageGUID
        {
            get;
            set;
        }

        /// <summary>
        /// Set of interests for the person
        /// </summary>
        public string[] Interests
        {
            get
            {
                List<string> resultsList = new List<string>();

                using (var context = new PeopleSearchContext())
                {
                    var interestsQuery = context.Interest.Where(interest => interest.Person.PersonId == this.PersonId);

                    foreach (Interest interest in interestsQuery)
                    {
                        resultsList.Add(interest.TheInterest);
                    }
                }

                return resultsList.ToArray();
            }
        }

        #region Address

        /// <summary>
        /// Address Line 1
        /// </summary>
        [Display(Name = "Address Line 1")]
        public string AddressLine1
        {
            get;
            set;
        }

        /// <summary>
        /// Address Line 2
        /// </summary>
        [Display(Name = "Address Line 2")]
        public string AddressLine2
        {
            get;
            set;
        }

        /// <summary>
        /// City of Residence
        /// </summary>
        public string City
        {
            get;
            set;
        }

        /// <summary>
        /// State or Territory of Residence
        /// </summary>
        [Display(Name = "State or Territory")]
        public string StateOrTerritory
        {
            get;
            set;
        }

        [Display(Name = "Zip Code")]
        public string ZipCode
        {
            get;
            set;
        }

        /// <summary>
        /// Country of Residence
        /// </summary>
        public string Country
        {
            get;
            set;
        }

        #endregion Address

        #region Address Formatting

        /// <summary>
        /// Format the address using the parts seperator passed in ('\n', '<br/>', ','  ...)
        /// </summary>
        /// <param name="lf">Line Separator</param>
        /// <returns>Formatted address</returns>
        private string FormatAddress(string lf)
        {
            StringBuilder results = new StringBuilder();

            string cityState = FormatAddressPart(City, (string.IsNullOrEmpty(StateOrTerritory) ? "" : ", ")) +
                FormatAddressPart(StateOrTerritory, "");

            results.Append(FormatAddressPart(AddressLine1, lf));
            results.Append(FormatAddressPart(AddressLine2, lf));
            results.Append(FormatAddressPart(cityState, lf));
            results.Append(FormatAddressPart(ZipCode, lf));
            results.Append(Country);

            return results.ToString();
        }

        /// <summary>
        /// IF there is text in the address part, then return the text followed by the separator.
        /// Otherwise return the empty string.
        /// </summary>
        /// <param name="part">Part of the address to go on one line</param>
        /// <param name="linefeed">Line parts separator</param>
        /// <returns>Formatted address with separator in it.</returns>
        private string FormatAddressPart(string part, string linefeed)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                return "";
            }
            else
            {
                return $"{part}{linefeed}";
            }
        }

        #endregion


        /// <summary>
        /// Address of the preson as Line Separated string
        /// </summary>
        [Display(Name = "Address")]
        [NotMapped]
        public string AddressAsText
        {
            get
            {
                return FormatAddress( Environment.NewLine );
            }
        }
        /// <summary>
        /// Address of the preson as HTML
        /// </summary>
        [Display(Name = "Address")]
        [NotMapped]
        public string AddressAsHTML
        {
            get
            {
                return FormatAddress( "<br/>" );
            }
        }

        /// <summary>
        /// Address of the preson as HTML
        /// </summary>
        [Display(Name = "Address")]
        [NotMapped]
        public string AddressCommaSeparated
        {
            get
            {
                return FormatAddress(", ");
            }
        }

        /// <summary>
        /// Constructor... Create an empty person
        /// </summary>
        public Person ()
        {
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.AddressLine1 = string.Empty;
            this.AddressLine2 = string.Empty;
            this.BirthDate = new DateTime ( 2000, 1, 1 );
            this.City = string.Empty;
            this.Country = string.Empty;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.StateOrTerritory = string.Empty;
            this.ZipCode = string.Empty;
        }

        /// <summary>
        /// String representation of person for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Person: {FirstName} {LastName} : DOB: {DisplayBirthDate}";
        }
    }
}
