﻿// Models/ViewModels/InventoryViewModel.cs
namespace InsightOps.Monolith.Models.ViewModels
{
    public class InventoryViewModel
    {
        public List<Inventory.InventoryItem> Items { get; set; } = new List<Inventory.InventoryItem>();
        public Inventory.InventoryItem? NewItem { get; set; }
    }
}