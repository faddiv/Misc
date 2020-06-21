using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinAuth.Data
{
    public class Entities : DbContext
    {
        public Entities()
        {

        }

        public Entities(DbContextOptions<Entities> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
    }
}
