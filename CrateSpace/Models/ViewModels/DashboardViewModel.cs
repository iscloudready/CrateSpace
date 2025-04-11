// Models/ViewModels/DashboardViewModel.cs
namespace cratespace.Monolith.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalInventoryItems { get; set; }
        public int LowStockItemsCount { get; set; }
        public decimal TotalInventoryValue { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrdersCount { get; set; }
        public decimal TotalOrdersValue { get; set; }

        public List<CrateSpace.Models.Inventory.LowStockAlert> LowStockAlerts { get; set; } = new List<CrateSpace.Models.Inventory.LowStockAlert>();
        public List<CrateSpace.Models.Order.Order> RecentOrders { get; set; } = new List<CrateSpace.Models.Order.Order>();
    }
}