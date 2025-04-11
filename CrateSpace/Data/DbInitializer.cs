using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using CrateSpace.Models.Inventory;
using CrateSpace.Models.Order;

namespace CrateSpace.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(cratespaceDbContext context, ILogger logger)
        {
            try
            {
                // Create schema and set search path
                await context.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS cratespace;");
                await context.Database.ExecuteSqlRawAsync("SET search_path TO cratespace,public;");

                // Only try to create the migrations history table once
                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS cratespace.__EFMigrationsHistory (
                        MigrationId character varying(150) NOT NULL,
                        ProductVersion character varying(32) NOT NULL,
                        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
                    );");
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P07")
                {
                    logger.LogInformation("Migration history table already exists");
                }

                // Check for pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingMigrationsList = pendingMigrations.ToList();

                if (pendingMigrationsList.Any())
                {
                    logger.LogInformation("Found {Count} pending migrations: {@Migrations}",
                        pendingMigrationsList.Count,
                        pendingMigrationsList);

                    try
                    {
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Successfully applied pending migrations");
                    }
                    catch (PostgresException pgEx) when (pgEx.SqlState == "42P07")
                    {
                        logger.LogInformation("Tables already exist, continuing...");
                    }
                }

                // Check if inventory data needs to be seeded
                if (!await context.InventoryItems.AnyAsync())
                {
                    logger.LogInformation("Initializing inventory seed data...");

                    var inventoryItems = new[]
                    {
                        new InventoryItem
                        {
                            Name = "Widget A",
                            Quantity = 100,
                            Price = 19.99M,
                            MinimumQuantity = 20,
                            LastRestocked = DateTime.UtcNow
                        },
                        new InventoryItem
                        {
                            Name = "Widget B",
                            Quantity = 50,
                            Price = 29.99M,
                            MinimumQuantity = 10,
                            LastRestocked = DateTime.UtcNow
                        },
                        new InventoryItem
                        {
                            Name = "Gadget X",
                            Quantity = 75,
                            Price = 49.99M,
                            MinimumQuantity = 15,
                            LastRestocked = DateTime.UtcNow
                        }
                    };

                    await context.InventoryItems.AddRangeAsync(inventoryItems);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Inventory seed data initialized successfully");
                }
                else
                {
                    logger.LogInformation("Inventory data already exists");
                }

                // Check if order data needs to be seeded
                if (!await context.Orders.AnyAsync())
                {
                    logger.LogInformation("Initializing order seed data...");

                    // Add sample orders
                    var orders = new[]
                    {
                        new Order
                        {
                            ItemName = "Widget A",
                            Quantity = 5,
                            TotalPrice = 99.95M,
                            Status = "Completed",
                            OrderDate = DateTime.UtcNow.AddDays(-2)
                        },
                        new Order
                        {
                            ItemName = "Gadget X",
                            Quantity = 3,
                            TotalPrice = 149.97M,
                            Status = "Pending",
                            OrderDate = DateTime.UtcNow.AddHours(-3)
                        }
                    };

                    await context.Orders.AddRangeAsync(orders);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Order seed data initialized successfully");
                }
                else
                {
                    logger.LogInformation("Order data already exists");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization");
                throw new Exception("Database initialization failed", ex);
            }
        }

        private static async Task EnsureTableExists(cratespaceDbContext context, string tableName, ILogger logger)
        {
            try
            {
                // Check if table exists
                var exists = await context.Database.ExecuteSqlRawAsync(
                    $"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = '{tableName}')");

                if (exists == 0)
                {
                    logger.LogWarning("Table {TableName} does not exist", tableName);
                    // Let migrations handle table creation
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking table existence: {TableName}", tableName);
                throw;
            }
        }
    }
}