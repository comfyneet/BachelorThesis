using Microsoft.EntityFrameworkCore;

namespace RiceDoctor.DatabaseManager
{
    public class RiceContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Website> Websites { get; set; }

        public DbSet<Association> Associations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=..\..\..\..\Resources\rice.db");
        }
    }
}