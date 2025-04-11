// Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using InsightOps.Monolith.Interfaces;
using InsightOps.Monolith.Models.Order;
using InsightOps.Monolith.Models.ViewModels;
using System.Diagnostics;

namespace InsightOps.Monolith.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            IInventoryService inventoryService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                var inventory = await _inventoryService.GetAllItemsAsync();

                var viewModel = new OrderViewModel
                {
                    Orders = orders.ToList(),
                    AvailableItems = inventory.ToList(),
                    NewOrder = new CreateOrderDto()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders list");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var orderStatus = await _orderService.GetOrderStatusAsync(id);
                return View(orderStatus);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for ID: {OrderId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var inventory = await _inventoryService.GetAllItemsAsync();
                ViewBag.InventoryItems = inventory;
                return View(new CreateOrderDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create order form");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto orderDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _orderService.PlaceOrderAsync(orderDto);
                    if (response.Success)
                    {
                        TempData["Success"] = "Order created successfully";
                        return RedirectToAction(nameof(Details), new { id = response.OrderId });
                    }
                    else
                    {
                        ModelState.AddModelError("", response.Message);
                    }
                }

                // If we get here, there was a problem
                var inventory = await _inventoryService.GetAllItemsAsync();
                ViewBag.InventoryItems = inventory;
                return View(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", "An error occurred while creating the order.");
                var inventory = await _inventoryService.GetAllItemsAsync();
                ViewBag.InventoryItems = inventory;
                return View(orderDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var success = await _orderService.CancelOrderAsync(id);
                if (success)
                {
                    TempData["Success"] = "Order cancelled successfully";
                }
                else
                {
                    TempData["Error"] = "Failed to cancel order";
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order, ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while cancelling the order.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    TempData["Error"] = "Status cannot be empty";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // We're using a repository method directly here as our service doesn't have this method
                // In a real-world application, you should consider adding this to the service layer
                var orderRepo = HttpContext.RequestServices.GetRequiredService<IOrderRepository>();
                var success = await orderRepo.UpdateOrderStatusAsync(id, status);

                if (success)
                {
                    TempData["Success"] = "Order status updated successfully";
                }
                else
                {
                    TempData["Error"] = "Failed to update order status";
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status, ID: {OrderId}, Status: {Status}", id, status);
                TempData["Error"] = "An error occurred while updating the order status.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}