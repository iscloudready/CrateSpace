// Controllers/HomeController.cs (continued)
using Microsoft.AspNetCore.Mvc;
using InsightOps.Monolith.Interfaces;
using InsightOps.Monolith.Models.ViewModels;
using System.Diagnostics;

namespace InsightOps.Monolith.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IOrderService _orderService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IInventoryService inventoryService,
            IOrderService orderService,
            ILogger<HomeController> logger)
        {
            _inventoryService = inventoryService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var inventory = await _inventoryService.GetAllItemsAsync();
                var totalItems = inventory.Count();
                var totalValue = await _inventoryService.GetTotalInventoryValueAsync();

                var lowStockAlerts = await _inventoryService.GetLowStockAlertsAsync();
                var lowStockCount = lowStockAlerts.Count();

                var allOrders = await _orderService.GetAllOrdersAsync();
                var pendingOrdersCount = allOrders.Count(o => o.Status == "Pending");
                var totalOrdersValue = allOrders.Sum(o => o.TotalPrice);

                var viewModel = new DashboardViewModel
                {
                    TotalInventoryItems = totalItems,
                    TotalInventoryValue = totalValue,
                    LowStockItemsCount = lowStockCount,
                    LowStockAlerts = lowStockAlerts.ToList(),
                    TotalOrders = allOrders.Count(),
                    PendingOrdersCount = pendingOrdersCount,
                    TotalOrdersValue = totalOrdersValue,
                    RecentOrders = allOrders.OrderByDescending(o => o.OrderDate).Take(5).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Error view model class
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}