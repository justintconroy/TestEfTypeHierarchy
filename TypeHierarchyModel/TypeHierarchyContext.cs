using System.Data.Entity;
using TypeHierarchyModel.Models;

namespace TypeHierarchyModel
{
    public class TypeHierarchyContext : DbContext
    {
        public DbSet<BaseType> Bases { get; set; }
        //public DbSet<DerivedTypeA> As { get; set; }
        //public DbSet<OtherBaseType> Others { get; set; }
        public DbSet<DerivedTypeB> Bs { get; set; }
        public DbSet<DerivedOtherType> Others { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DerivedTypeA>();
            modelBuilder.Ignore<OtherBaseType>();
            //modelBuilder.Ignore<DerivedOtherType>();
            //modelBuilder.Ignore<DerivedTypeB>();

            modelBuilder.Entity<DerivedOtherType>()
                .ToTable("OtherBaseTypes");

            modelBuilder.Entity<DerivedTypeB>()
                .HasRequired(t => t.Other)
                .WithMany(t => t.Things)
                .HasForeignKey(t => t.OtherId);

            modelBuilder.Entity<DerivedTypeB>()
                .Property(t => t.OtherId)
                .HasColumnName("OtherId");

            modelBuilder.Entity<BaseType>()
                .ToTable("BaseType");
            modelBuilder.Entity<DerivedTypeB>()
                .ToTable("BaseType");
        }
    }
}
