using Microsoft.EntityFrameworkCore;
using SystemApi.Entities;

namespace SystemApi.DAL.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) 
        {
            

           
        }
        public DbSet<User> Users { get; set; }

    }
}
