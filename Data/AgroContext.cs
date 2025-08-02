using AGROPURE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AGROPURE.Data
{
    public class AgroContext : DbContext
    {
        public AgroContext(DbContextOptions<AgroContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            });

            // Product configurations
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
            });

            // Material configurations
            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Materials)
                      .HasForeignKey(e => e.SupplierId);
            });

            // ProductMaterial (BOM) configurations
            modelBuilder.Entity<ProductMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Materials)
                      .HasForeignKey(e => e.ProductId);
                entity.HasOne(e => e.Material)
                      .WithMany(m => m.ProductMaterials)
                      .HasForeignKey(e => e.MaterialId);
            });

            // Quote configurations
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Quotes)
                      .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Quotes)
                      .HasForeignKey(e => e.ProductId);
            });

            // Sale configurations
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Sales)
                      .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId);
                entity.HasOne(e => e.Quote)
                      .WithMany()
                      .HasForeignKey(e => e.QuoteId)
                      .IsRequired(false);
            });

            // Purchase configurations
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
                entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Purchases)
                      .HasForeignKey(e => e.SupplierId);
                entity.HasOne(e => e.Material)
                      .WithMany()
                      .HasForeignKey(e => e.MaterialId);
            });

            // Review configurations
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rating).IsRequired();
                entity.HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5");
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(e => e.ProductId);
            });

            // Supplier configurations
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
        }
    }
}