using Microsoft.AspNetCore.Mvc;
using InsightOps.Monolith.Models.ViewModels;

namespace InsightOps.Monolith.Controllers
{
    public class ServiceHealthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceHealthController> _logger;

        public ServiceHealthController(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<ServiceHealthController> logger)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new HealthStatusViewModel();

            try
            {
                // Application Service Group
                var applicationServices = new HealthStatusViewModel.ServiceGroup
                {
                    Name = "Application Services"
                };

                // Add the web application itself as a service
                applicationServices.Services.Add(new HealthStatusViewModel.ServiceHealth
                {
                    Name = "Web Application",
                    Status = "Healthy",
                    LastChecked = DateTime.UtcNow,
                    Details = "Application is running",
                    Metrics = new Dictionary<string, string>
                    {
                        ["Uptime"] = GetUptime(),
                        ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                        ["Version"] = "1.0.0"
                    }
                });

                // Add database health check
                applicationServices.Services.Add(await CheckDatabaseHealthAsync());

                model.ServiceGroups.Add(applicationServices);

                // System Resources Group
                var systemResources = new HealthStatusViewModel.ServiceGroup
                {
                    Name = "System Resources"
                };

                // Add system health checks
                systemResources.Services.Add(GetSystemHealth());

                model.ServiceGroups.Add(systemResources);

                model.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking services health");
                TempData["Error"] = "Failed to check services health";
            }

            return View(model);
        }

        [HttpGet("details/{serviceName}")]
        public IActionResult Details(string serviceName)
        {
            // This would normally fetch detailed information about the service
            return Json(new
            {
                details = $"Detailed information for {serviceName}\nStatus: Healthy\nUptime: {GetUptime()}\nNo issues detected."
            });
        }

        [HttpGet("metrics/{serviceName}")]
        public IActionResult Metrics(string serviceName)
        {
            // This would normally fetch metrics for the service
            var metrics = new Dictionary<string, string>();

            switch (serviceName)
            {
                case "Web Application":
                    metrics = new Dictionary<string, string>
                    {
                        ["Requests/Second"] = "24.5",
                        ["Average Response Time"] = "85ms",
                        ["Memory Usage"] = "256MB",
                        ["Active Connections"] = "12"
                    };
                    break;
                case "Database":
                    metrics = new Dictionary<string, string>
                    {
                        ["Connections"] = "8",
                        ["Queries/Second"] = "45.2",
                        ["Average Query Time"] = "12ms",
                        ["Cache Hit Rate"] = "92%"
                    };
                    break;
                case "System":
                    metrics = new Dictionary<string, string>
                    {
                        ["CPU Usage"] = "34%",
                        ["Memory Usage"] = "68%",
                        ["Disk Space"] = "42%",
                        ["Network Traffic"] = "2.4MB/s"
                    };
                    break;
                default:
                    metrics = new Dictionary<string, string>
                    {
                        ["Status"] = "Unknown service"
                    };
                    break;
            }

            return Json(metrics);
        }

        [HttpGet("refresh")]
        public IActionResult Refresh()
        {
            return RedirectToAction(nameof(Index));
        }

        private async Task<HealthStatusViewModel.ServiceHealth> CheckDatabaseHealthAsync()
        {
            try
            {
                // In a real application, we would check the database connection
                // For now, we'll simulate a successful check
                await Task.Delay(100); // Simulate async operation

                return new HealthStatusViewModel.ServiceHealth
                {
                    Name = "Database",
                    Status = "Healthy",
                    LastChecked = DateTime.UtcNow,
                    Details = "Database connection is stable",
                    Metrics = new Dictionary<string, string>
                    {
                        ["Connection Pool"] = "Active",
                        ["Connections"] = "8/100",
                        ["Latency"] = "12ms"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");

                return new HealthStatusViewModel.ServiceHealth
                {
                    Name = "Database",
                    Status = "Unhealthy",
                    LastChecked = DateTime.UtcNow,
                    Details = $"Database connection failed: {ex.Message}"
                };
            }
        }

        private HealthStatusViewModel.ServiceHealth GetSystemHealth()
        {
            try
            {
                // In a real application, we would collect system metrics
                // For now, we'll return simulated data

                return new HealthStatusViewModel.ServiceHealth
                {
                    Name = "System",
                    Status = "Healthy",
                    LastChecked = DateTime.UtcNow,
                    Details = "System resources are within normal parameters",
                    Metrics = new Dictionary<string, string>
                    {
                        ["CPU Usage"] = "34%",
                        ["Memory Usage"] = "68%",
                        ["Disk Space"] = "42%",
                        ["Network"] = "Stable"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System health check failed");

                return new HealthStatusViewModel.ServiceHealth
                {
                    Name = "System",
                    Status = "Unhealthy",
                    LastChecked = DateTime.UtcNow,
                    Details = $"System health check failed: {ex.Message}"
                };
            }
        }

        private string GetUptime()
        {
            var startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
            var uptime = DateTime.Now - startTime;

            if (uptime.TotalDays >= 1)
                return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
            else if (uptime.TotalHours >= 1)
                return $"{(int)uptime.TotalHours}h {uptime.Minutes}m {uptime.Seconds}s";
            else if (uptime.TotalMinutes >= 1)
                return $"{(int)uptime.TotalMinutes}m {uptime.Seconds}s";
            else
                return $"{(int)uptime.TotalSeconds}s";
        }
    }
}