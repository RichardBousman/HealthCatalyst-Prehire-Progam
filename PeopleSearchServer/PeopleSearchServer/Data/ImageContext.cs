using Microsoft.EntityFrameworkCore;

namespace PeopleSearchServer.Models
{
    public class ImageContext : DbContext
    {
        public static string ConnectionString;

        public ImageContext ( DbContextOptions<ImageContext> options)
            : base(options)
        {
        }


        public ImageContext ()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if ( !optionsBuilder.IsConfigured )
            {
                optionsBuilder.UseSqlServer ( ConnectionString );
            }
        }

        public DbSet<PeopleSearchServer.Models.Image> Image { get; set; }
    }
}
