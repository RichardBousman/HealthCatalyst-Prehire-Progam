using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PeopleSearchServer.Models
{
    public class PeopleSearchContext : DbContext
    {
        public static string ConnectionString;

        public PeopleSearchContext (DbContextOptions<PeopleSearchContext> options)
            : base(options)
        {
        }


        public PeopleSearchContext ()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if ( !optionsBuilder.IsConfigured )
            {
                optionsBuilder.UseSqlServer ( ConnectionString );
            }
        }

        public DbSet<PeopleSearchServer.Models.Person> Person { get; set; }

        public DbSet<Interest> Interest { get; set; }
    }
}
