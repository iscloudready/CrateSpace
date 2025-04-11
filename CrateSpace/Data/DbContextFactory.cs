using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.IO;

namespace CrateSpace.Data
{
    public class DbContextFactory : IDesignTimeDbContextFactory<cratespaceDbContext>
    {
        public cratespaceDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Create the database if it doesn't exist
            EnsureDatabaseExists(connectionString);

            var optionsBuilder = new DbContextOptionsBuilder<cratespaceDbContext>();
            optionsBuilder.UseNpgsql(connectionString, b =>
                b.MigrationsHistoryTable("__EFMigrationsHistory", "cratespace"));

            return new cratespaceDbContext(
                optionsBuilder.Options,
                new LoggerFactory().CreateLogger<cratespaceDbContext>());
        }

        private void EnsureDatabaseExists(string connectionString)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var dbName = connectionStringBuilder.Database;
            var username = connectionStringBuilder.Username;
            var password = connectionStringBuilder.Password;
            var host = connectionStringBuilder.Host;
            var port = connectionStringBuilder.Port;

            // Create a connection string to the postgres database to create the user and the app database
            var masterConnectionString = $"Host={host};Port={port};Database=postgres;Username=postgres;Password={password}";

            try
            {
                // These operations need to happen outside of the application's DbContext
                using var connection = new NpgsqlConnection(masterConnectionString);
                connection.Open();

                Console.WriteLine("Connected to PostgreSQL server...");

                // Check if user exists and create if not
                using (var checkUserCmd = new NpgsqlCommand($"SELECT 1 FROM pg_roles WHERE rolname = '{username}'", connection))
                {
                    var userExists = checkUserCmd.ExecuteScalar() != null;

                    if (!userExists)
                    {
                        Console.WriteLine($"Creating user {username}...");
                        using var createUserCmd = new NpgsqlCommand($"CREATE USER {username} WITH PASSWORD '{password}'", connection);
                        createUserCmd.ExecuteNonQuery();
                        Console.WriteLine($"User {username} created successfully");
                    }
                    else
                    {
                        Console.WriteLine($"User {username} already exists");
                    }
                }

                // Check if database exists and create if not
                using (var checkDbCmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{dbName}'", connection))
                {
                    var dbExists = checkDbCmd.ExecuteScalar() != null;

                    if (!dbExists)
                    {
                        Console.WriteLine($"Creating database {dbName}...");
                        using var createDbCmd = new NpgsqlCommand($"CREATE DATABASE {dbName} OWNER {username}", connection);
                        createDbCmd.ExecuteNonQuery();
                        Console.WriteLine($"Database {dbName} created successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Database {dbName} already exists");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database initialization: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            // Now connect to the application database to create schema
            try
            {
                using var appDbConnection = new NpgsqlConnection(connectionString);
                appDbConnection.Open();

                Console.WriteLine($"Connected to application database {dbName}...");

                // Create schema if it doesn't exist
                using (var createSchemaCmd = new NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS cratespace", appDbConnection))
                {
                    createSchemaCmd.ExecuteNonQuery();
                    Console.WriteLine("Schema 'cratespace' created or already exists");
                }

                // Grant privileges
                using (var grantSchemaCmd = new NpgsqlCommand($"GRANT ALL PRIVILEGES ON SCHEMA cratespace TO {username}", appDbConnection))
                {
                    grantSchemaCmd.ExecuteNonQuery();
                    Console.WriteLine($"Granted schema privileges to {username}");
                }

                // Set default privileges
                using (var defaultPrivilegesCmd = new NpgsqlCommand($"ALTER DEFAULT PRIVILEGES IN SCHEMA cratespace GRANT ALL ON TABLES TO {username}", appDbConnection))
                {
                    defaultPrivilegesCmd.ExecuteNonQuery();
                    Console.WriteLine("Default privileges set");
                }

                Console.WriteLine("Database setup completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during schema setup: {ex.Message}");
                // Log but continue - the migration might still work
            }
        }
    }
}