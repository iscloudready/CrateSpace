// Models/Order/Order.cs
namespace InsightOps.Monolith.Models.Order
{
    public class Order
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";

        // Optional: Add any business logic or validation
        public void CalculateTotalPrice(decimal unitPrice)
        {
            TotalPrice = Quantity * unitPrice;
        }

        public bool CanBeFulfilled(int availableQuantity)
        {
            return Quantity <= availableQuantity;
        }
    }
}