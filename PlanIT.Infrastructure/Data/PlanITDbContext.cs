using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanIT.Domain;
using Microsoft.EntityFrameworkCore;

namespace PlanIT.Infrastructure.Data
{
    public class PlanITDbContext : DbContext
    {
        public PlanITDbContext(DbContextOptions<PlanITDbContext> options) : base(options)
        {
        }

        public DbSet<Travel> Travels { get; set; }


        public DbSet<User> Users { get; set; }



        //Aca engau irian otros DbSet de otras entidades (Activities, etc)
    }
}
