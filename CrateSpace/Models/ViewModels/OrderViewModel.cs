// Models/ViewModels/OrderViewModel.cs
namespace InsightOps.Monolith.Models.ViewModels
{
    public class OrderViewModel
    {
        public List<Order.Order> Orders { get; set; } = new List<Order.Order>();
        public List<Inventory.InventoryItem> AvailableItems { get; set; } = new List<Inventory.InventoryItem>();
        public Order.CreateOrderDto? NewOrder { get; set; }
    }
}