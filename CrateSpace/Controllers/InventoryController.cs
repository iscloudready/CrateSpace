// Controllers/InventoryController.cs
using Microsoft.AspNetCore.Mvc;
using InsightOps.Monolith.Interfaces;
using InsightOps.Monolith.Models.Inventory;
using InsightOps.Monolith.Models.ViewModels;
using System.Diagnostics;

namespace InsightOps.Monolith.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var items = await _inventoryService.GetAllItemsAsync();
                var viewModel = new InventoryViewModel
                {
                    Items = items.ToList(),
                    NewItem = new InventoryItem()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory list");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var item = await _inventoryService.GetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory item details for ID: {ItemId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new InventoryItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryItem item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _inventoryService.CreateItemAsync(item);
                    TempData["Success"] = "Item created successfully";
                    return RedirectToAction(nameof(Index));
                }
                return View(item);
            }
            catch (InvalidOperationException ex)
            {
                // This exception is thrown when item name already exists
                ModelState.AddModelError("Name", ex.Message);
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory item");
                ModelState.AddModelError("", "An error occurred while creating the item.");
                return View(item);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var item = await _inventoryService.GetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory item for edit, ID: {ItemId}", id);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryItem item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _inventoryService.UpdateItemAsync(item);
                    TempData["Success"] = "Item updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                return View(item);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // This exception is thrown when item name already exists
                ModelState.AddModelError("Name", ex.Message);
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory item, ID: {ItemId}", id);
                ModelState.AddModelError("", "An error occurred while updating the item.");
                return View(item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _inventoryService.DeleteItemAsync(id);
                TempData["Success"] = "Item deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory item, ID: {ItemId}", id);
                TempData["Error"] = "An error occurred while deleting the item.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            try
            {
                if (quantity < 0)
                {
                    ModelState.AddModelError("", "Quantity cannot be negative.");
                    var item = await _inventoryService.GetItemByIdAsync(id);
                    return View("Details", item);
                }

                var success = await _inventoryService.UpdateStockAsync(id, quantity);
                if (success)
                {
                    TempData["Success"] = "Stock updated successfully";
                }
                else
                {
                    TempData["Error"] = "Item not found";
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for item, ID: {ItemId}", id);
                TempData["Error"] = "An error occurred while updating the stock.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LowStock()
        {
            try
            {
                var alerts = await _inventoryService.GetLowStockAlertsAsync();
                return View(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading low stock alerts");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
