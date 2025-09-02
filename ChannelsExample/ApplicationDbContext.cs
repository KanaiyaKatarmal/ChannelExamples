using ChannelsExample.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ChannelsExample
{
  

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
    }
}
