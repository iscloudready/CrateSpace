﻿// Models/Order/Responses/OrderResponse.cs
namespace CrateSpace.Models.Order.Responses
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
