using Microsoft.EntityFrameworkCore;

namespace sample_dotnet_webapi.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        public DbSet<Person> Persons { get; set; } = null!;
    }
}
