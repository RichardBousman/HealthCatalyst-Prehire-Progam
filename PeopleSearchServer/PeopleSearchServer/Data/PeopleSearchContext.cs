using Microsoft.EntityFrameworkCore;

namespace PeopleSearchServer.Models
{
    /// <summary>
    /// Database Context for the database search
    /// </summary>
    public class PeopleSearchContext : DbContext
    {
        /// <summary>
        /// Connection string to use
        /// </summary>
        public static string ConnectionString;

        /// <summary>
        /// Primary constructor used by ASP.NET
        /// </summary>
        /// <param name="options"></param>
        public PeopleSearchContext (DbContextOptions<PeopleSearchContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public PeopleSearchContext ()
        {
        }

        /// <summary>
        /// OnCOnfiguring overload called by the ASP.NET infrastructure
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if ( !optionsBuilder.IsConfigured )
            {
                optionsBuilder.UseSqlServer ( ConnectionString );
            }
        }

        /// <summary>
        /// Person database table accessor
        /// </summary>
        public DbSet<Person> Person { get; set; }

        /// <summary>
        /// Interest database table accessor
        /// </summary>
        public DbSet<Interest> Interest { get; set; }
    }
}
