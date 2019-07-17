using Microsoft.EntityFrameworkCore;

namespace PeopleSearchServer.Models
{
    public class ImageContext : DbContext
    {
        /// <summary>
        /// Connection string for the database connection
        /// </summary>
        public static string ConnectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Database Context Options</param>
        public ImageContext ( DbContextOptions<ImageContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ImageContext ()
        {
        }

        /// <summary>
        /// OnConfiguring call automatically called by the ASP.NET infrastructure
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
        /// Database reference
        /// </summary>
        public DbSet<PeopleSearchServer.Models.Image> Image { get; set; }
    }
}
