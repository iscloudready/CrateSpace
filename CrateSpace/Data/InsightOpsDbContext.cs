using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CrateSpace.Models.Inventory;
using CrateSpace.Models.Order;
using CrateSpace.Models.Inventory;
using CrateSpace.Models.Order;

namespace CrateSpace.Data
{
    public class InsightOpsDbContext : DbContext
    {
        private readonly ILogger<InsightOpsDbContext> _logger;

        public InsightOpsDbContext(
            DbContextOptions<InsightOpsDbContext> options,
            ILogger<InsightOpsDbContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set default schema for all tables
            modelBuilder.HasDefaultSchema("insightops");

            try
            {
                // Configure Order entity
                modelBuilder.Entity<Order>(entity =>
                {
                    // Primary Key
                    entity.HasKey(e => e.Id);

                    // Properties
                    entity.Property(e => e.ItemName)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasComment("Name of the ordered item");
                    entity.Property(e => e.Quantity)
                        .IsRequired()
                        .HasComment("Quantity of items ordered");
                    entity.Property(e => e.TotalPrice)
                        .HasColumnType("decimal(18,2)")
                        .IsRequired()
                        .HasComment("Total price of the order");
                    entity.Property(e => e.Status)
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasDefaultValue("Pending")
                        .HasComment("Current status of the order");
                    entity.Property(e => e.OrderDate)
                        .IsRequired()
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasComment("Date and time when the order was created");

                    // Indexes for better query performance
                    entity.HasIndex(e => e.Status)
                        .HasDatabaseName("IX_Orders_Status");
                    entity.HasIndex(e => e.OrderDate)
                        .HasDatabaseName("IX_Orders_OrderDate");

                    // Table configuration
                    entity.ToTable("Orders", schema: "insightops", b => b.HasComment("Stores all order information"));
                });

                // Configure InventoryItem entity
                modelBuilder.Entity<InventoryItem>(entity =>
                {
                    // Primary Key
                    entity.HasKey(e => e.Id);

                    // Properties
                    entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasComment("Name of the inventory item");

                    entity.Property(e => e.Quantity)
                        .IsRequired()
                        .HasComment("Current quantity in stock");

                    entity.Property(e => e.Price)
                        .HasColumnType("decimal(18,2)")
                        .IsRequired()
                        .HasComment("Price of the item");

                    entity.Property(e => e.LastRestocked)
                        .IsRequired()
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasComment("Last restock date and time");

                    entity.Property(e => e.MinimumQuantity)
                        .IsRequired()
                        .HasDefaultValue(10)
                        .HasComment("Minimum quantity threshold for reordering");

                    // Indexes
                    entity.HasIndex(e => e.Name)
                        .IsUnique()
                        .HasDatabaseName("IX_InventoryItems_Name");

                    entity.HasIndex(e => e.Quantity)
                        .HasDatabaseName("IX_InventoryItems_Quantity");

                    // Table configuration
                    entity.ToTable("InventoryItems", schema: "insightops", tb =>
                    {
                        tb.HasComment("Stores inventory item information");
                    });
                });

                // Add relationship between Order and InventoryItem (based on ItemName)
                // This is an enhancement over the original microservices where they were not directly related
                modelBuilder.Entity<Order>()
                    .HasIndex(o => o.ItemName)
                    .HasDatabaseName("IX_Orders_ItemName");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during model creation");
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw new DbUpdateException("Failed to save changes to the database", ex);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableDetailedErrors(false);
        }
    }
}