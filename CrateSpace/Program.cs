using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using CrateSpace.Data;
using CrateSpace.Interfaces;
using CrateSpace.Repositories;
using CrateSpace.Services;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);
// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add services to the container
builder.Services.AddControllersWithViews();
// Configure database
builder.Services.AddDbContext<cratespaceDbContext>((serviceProvider, options) =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<cratespaceDbContext>>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "cratespace");
    });
});
// Register repositories
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Register services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<cratespaceDbContext>("database", HealthStatus.Unhealthy);
var app = builder.Build();
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            }),
            totalDuration = report.TotalDuration
        };
        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
});
// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<cratespaceDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        await DbInitializer.InitializeAsync(context, logger);
        logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}
// Run the application
await app.RunAsync();