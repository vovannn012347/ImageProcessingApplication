using System.Data.Entity;

using ImageProcessingApplication.Data.Entities;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        //Additional DbSets for your entities can go here
        public DbSet<UserLimit> Limits { get; set; }
        public DbSet<ProcessOperation> ProcessOperations { get; set; }
        public DbSet<OperationOrder> OperationOrderings { get; set; }

        public DbSet<CompositeProcessRequest> ProcessRequest { get; set; }
        public DbSet<ProcessRequest> ChildProcessRequests { get; set; }
        public DbSet<ImageFile> ImageFiles { get; set; }

        public DbSet<AppSettings> AppSettings { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Limits)
                .WithRequired(l => l.User)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<OperationOrder>()
                .HasRequired(uf => uf.NextOperation)
                .WithMany(u => u.PreviousOperations)
                .HasForeignKey(uf => uf.NextOperationId)
                .WillCascadeOnDelete(false);  // Avoids circular delete rules

            modelBuilder.Entity<OperationOrder>()
                .HasRequired(uf => uf.PreviousOperation)
                .WithMany(u => u.NextOperations)
                .HasForeignKey(uf => uf.PreviousOperationId)
                .WillCascadeOnDelete(false);  // Avoids circular delete rules

            modelBuilder.Entity<CompositeProcessRequest>()
                .HasMany(u => u.ChildRequests)
                .WithRequired(l => l.ParentRequest)
                .HasForeignKey(l => l.ParentRequestId);


            base.OnModelCreating(modelBuilder);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}