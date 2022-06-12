using Microsoft.EntityFrameworkCore;

namespace SlowlyChangingDimensions
{
    public class ScdDbContext1 : DbContext
    {
        public ScdDbContext1() : base()
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=SlowlyChangingDimensions1;Trusted_Connection=True;");
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ScdExampleTable1>(entity =>
            {
                entity.HasOne(e => e.PreviousVersion)
                    .WithOne(e => e.NextVersion)
                    .HasForeignKey<ScdExampleTable1>(e => e.PreviousVersionId);
                //entity.HasIndex(e => new { e.Id, e.CreatedTimestamp });
            });

            modelBuilder.Entity<ScdExampleTable2>(entity =>
            {
                //entity.HasIndex(e => new { e.CreatedTimestamp, e.EndTimestamp });
            });
        }

        public DbSet<ScdExampleTable1> ScdExampleTable1 { get; set; }

        public DbSet<ScdExampleTable2> ScdExampleTable2 { get; set; }
    }
}
