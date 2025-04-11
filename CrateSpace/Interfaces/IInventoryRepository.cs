// Interfaces/IInventoryRepository.cs
using CrateSpace.Models.Inventory;

namespace CrateSpace.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<InventoryItem>> GetAllItemsAsync();
        Task<InventoryItem?> GetItemByIdAsync(int id);
        Task<InventoryItem?> GetItemByNameAsync(string name);
        Task<InventoryItem> CreateItemAsync(InventoryItem item);
        Task<InventoryItem> UpdateItemAsync(InventoryItem item);
        Task<bool> DeleteItemAsync(int id);
        Task<bool> UpdateStockAsync(int id, int quantity);
        Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync();
        Task<bool> ItemExistsAsync(int id);
        Task<bool> ItemExistsByNameAsync(string name);
        Task<IEnumerable<InventoryItem>> GetItemsWithStockBelowAsync(int threshold);
        Task<int> GetTotalUniqueItemsAsync();
        Task<int> GetTotalStockQuantityAsync();
        Task<decimal> GetTotalInventoryValueAsync();
    }
}