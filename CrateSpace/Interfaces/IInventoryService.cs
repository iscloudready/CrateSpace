// Interfaces/IInventoryService.cs
using InsightOps.Monolith.Models.Inventory;

namespace InsightOps.Monolith.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryItem>> GetAllItemsAsync();
        Task<InventoryItem?> GetItemByIdAsync(int id);
        Task<InventoryItem> CreateItemAsync(InventoryItem item);
        Task<InventoryItem> UpdateItemAsync(InventoryItem item);
        Task<bool> DeleteItemAsync(int id);
        Task<bool> UpdateStockAsync(int id, int quantity);
        Task<bool> CheckAvailabilityAsync(string itemName, int quantity);
        Task<bool> ReserveStockAsync(string itemName, int quantity);
        Task<IEnumerable<LowStockAlert>> GetLowStockAlertsAsync();
        Task<decimal> GetTotalInventoryValueAsync();
    }
}