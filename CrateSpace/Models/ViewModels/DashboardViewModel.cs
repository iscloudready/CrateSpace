// Models/ViewModels/DashboardViewModel.cs
using InsightOps.Monolith.Models.Inventory;

namespace InsightOps.Monolith.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalInventoryItems { get; set; }
        public int LowStockItemsCount { get; set; }
        public decimal TotalInventoryValue { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrdersCount { get; set; }
        public decimal TotalOrdersValue { get; set; }

        public List<LowStockAlert> LowStockAlerts { get; set; } = new List<LowStockAlert>();
        public List<Order.Order> RecentOrders { get; set; } = new List<Order.Order>();
    }
}