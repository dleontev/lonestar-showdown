using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace LonestarShowdown.Database
{
    //[DbConfigurationType(typeof (MyDbConfiguration))]
    public class LonestarDbContext : DbContext
    {
        public LonestarDbContext()
            : base("LonestarDBContext")
        {
        }

        public virtual DbSet<Personnel> Personnels { get; set; }
        public virtual DbSet<PlayerStat> PlayerStats { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }
        public virtual DbSet<SecurityQuestion> SecurityQuestions { get; set; }
        public virtual DbSet<Squad> Squads { get; set; }
        public virtual DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Position>()
                .HasMany(e => e.Personnels)
                .WithOptional(e => e.Position1)
                .HasForeignKey(e => e.Position);

            modelBuilder.Entity<SecurityQuestion>()
                .HasMany(e => e.Personnels)
                .WithRequired(e => e.SecurityQuestion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Team>()
                .HasMany(e => e.Personnels)
                .WithOptional(e => e.Team)
                .HasForeignKey(e => e.TeamID);

            modelBuilder.Entity<Team>()
                .HasMany(e => e.Personnels1)
                .WithOptional(e => e.Team1)
                .HasForeignKey(e => e.TeamRequested);
        }
    }

    //public class MyDbConfiguration : DbConfiguration
    //{
    //    public MyDbConfiguration()
    //    {
    //        SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
    //    }
    //}
}