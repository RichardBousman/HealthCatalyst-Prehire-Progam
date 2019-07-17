using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace PeopleSearchServer.Models
{
    /// <summary>
    /// Class to seed the database
    /// </summary>
    public class SeedData
    {
        /// <summary>
        /// Seed the Database
        /// </summary>
        /// <param name="serviceProvider"></param>
         public static void Initialize ( DbContextOptions<ImageContext> imageDbOptions,
                                        DbContextOptions<PeopleSearchContext> peopleDbContext )
        {
            Image imageUnknown;
            Image imageRichard;
            Image imageTrent;

            // Create the Image Database Entries
            // Using the Context object
            using ( var context = new ImageContext ( imageDbOptions ) )
            {
                // Check for anybody already in the database
                if ( context.Image.Any () )
                {
                    // Database already seeded, exit
                    return;
                }

                // Create Images

                imageUnknown = new Image ( @".\SeedData\Unknown.jpg" )
                {
                    Id = "0"
                };
                imageRichard = new Image ( @".\SeedData\Richard.jpg" ) { NumberPeopleAssignedTo = 1 };
                imageTrent = new Image ( @".\SeedData\Trent.jpg" ) { NumberPeopleAssignedTo = 1 };

                context.Image.AddRange ( imageUnknown, imageRichard, imageTrent );
                context.SaveChanges ();
            }

            // Using the Context object
            using ( var context = new PeopleSearchContext ( peopleDbContext ) )
            {
                // Check for anybody already in the database
                if (context.Person.Any())
                {
                    // Database already seeded, exit
                    return;
                }

                Person richard = new Person
                {
                    FirstName = "Richard",
                    LastName = "Bousman",
                    BirthDate = new DateTime(1990, 10, 17),
                    ImageGUID = imageRichard.Id,
                    AddressLine1 = "2880 Sandestin",
                    AddressLine2 = "",
                    City = "Reno",
                    StateOrTerritory = "Nevada",
                    ZipCode = "89523",
                    Country = "USA"
                };

                Person trent = new Person
                {
                    FirstName = "Trent",
                    LastName = "Wignall",
                    BirthDate = new DateTime(1989, 6, 19),
                    ImageGUID = imageTrent.Id,
                    AddressLine1 = "Corner Office",
                    AddressLine2 = "3165 Millrock Dr",
                    City = "Salt Lake City",
                    StateOrTerritory = "Utah",
                    ZipCode = "84121",
                    Country = "USA"
                };

                Person fred = new Person
                {
                    FirstName = "Fred",
                    LastName = "Flinestone",
                    BirthDate = new DateTime ( 0001, 5, 4 ),
                    ImageGUID = imageUnknown.Id,
                    AddressLine1 = "245 Rock Blvd",
                    AddressLine2 = "",
                    City = "Bedrock",
                    StateOrTerritory = "Stonehenge",
                    ZipCode = "00000",
                    Country = "Prehistoric"
                };

                // Add some People
                context.Person.AddRange(richard, trent, fred);

                context.Interest.AddRange(

                    new Interest
                    {
                        Person = richard,
                        TheInterest = "Programming"
                    },

                    new Interest
                    {
                        Person = richard,
                        TheInterest = "Baseball"
                    },

                    new Interest
                    {
                        Person = trent,
                        TheInterest = "Personnel"
                    },

                    new Interest
                    {
                        Person = trent,
                        TheInterest = "Cycling"
                    },

                    new Interest
                    {
                        Person = fred,
                        TheInterest = "Rock Climbing"
                    }
                );

                // Save new entries to database, assigning personId's
                context.SaveChanges();
            }
        }
    }
}
