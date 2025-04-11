// Services/InventoryService.cs
using CrateSpace.Interfaces;
using CrateSpace.Models.Inventory;

namespace CrateSpace.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IInventoryRepository inventoryRepository, ILogger<InventoryService> logger)
        {
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<InventoryItem>> GetAllItemsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all inventory items");
                return await _inventoryRepository.GetAllItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all inventory items");
                throw;
            }
        }

        public async Task<InventoryItem?> GetItemByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving inventory item with ID: {ItemId}", id);
                return await _inventoryRepository.GetItemByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving inventory item with ID: {ItemId}", id);
                throw;
            }
        }

        public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
        {
            try
            {
                _logger.LogInformation("Creating new inventory item: {ItemName}", item.Name);
                return await _inventoryRepository.CreateItemAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new inventory item");
                throw;
            }
        }

        public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
        {
            try
            {
                _logger.LogInformation("Updating inventory item with ID: {ItemId}", item.Id);
                return await _inventoryRepository.UpdateItemAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating inventory item with ID: {ItemId}", item.Id);
                throw;
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting inventory item with ID: {ItemId}", id);
                return await _inventoryRepository.DeleteItemAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting inventory item with ID: {ItemId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateStockAsync(int id, int quantity)
        {
            try
            {
                _logger.LogInformation("Updating stock for item with ID: {ItemId} to {Quantity}", id, quantity);
                return await _inventoryRepository.UpdateStockAsync(id, quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating stock for item with ID: {ItemId}", id);
                throw;
            }
        }

        public async Task<bool> CheckAvailabilityAsync(string itemName, int quantity)
        {
            try
            {
                _logger.LogInformation("Checking availability of {Quantity} units of {ItemName}", quantity, itemName);
                var item = await _inventoryRepository.GetItemByNameAsync(itemName);
                if (item == null)
                    return false;

                return item.Quantity >= quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for item {ItemName}", itemName);
                throw;
            }
        }

        public async Task<bool> ReserveStockAsync(string itemName, int quantity)
        {
            try
            {
                _logger.LogInformation("Attempting to reserve {Quantity} units of {ItemName}", quantity, itemName);
                var item = await _inventoryRepository.GetItemByNameAsync(itemName);
                if (item == null || item.Quantity < quantity)
                    return false;

                var newQuantity = item.Quantity - quantity;
                return await _inventoryRepository.UpdateStockAsync(item.Id, newQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for item {ItemName}", itemName);
                throw;
            }
        }

        public async Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving low stock alerts");
                var lowStockItems = await _inventoryRepository.GetLowStockItemsAsync();
                return lowStockItems.Select(item => new LowStockAlert
                {
                    ItemId = item.Id,
                    ItemName = item.Name,
                    CurrentQuantity = item.Quantity,
                    MinimumQuantity = item.MinimumQuantity,
                    Price = item.Price,
                    LastRestocked = item.LastRestocked
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving low stock alerts");
                throw;
            }
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            try
            {
                _logger.LogInformation("Calculating total inventory value");
                return await _inventoryRepository.GetTotalInventoryValueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total inventory value");
                throw;
            }
        }

        public async Task<InventoryItem?> GetItemByNameAsync(string name)
        {
            try
            {
                _logger.LogInformation("Retrieving inventory item by name: {ItemName}", name);
                return await _inventoryRepository.GetItemByNameAsync(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving inventory item by name: {ItemName}", name);
                throw;
            }
        }
    }
}