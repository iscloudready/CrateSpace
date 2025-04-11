// Interfaces/IOrderService.cs
using InsightOps.Monolith.Models.Order;
using InsightOps.Monolith.Models.Order.Responses;

namespace InsightOps.Monolith.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<OrderResponse> PlaceOrderAsync(CreateOrderDto orderDto);
        Task<bool> CancelOrderAsync(int orderId);
        Task<OrderStatusResponse> GetOrderStatusAsync(int orderId);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count);
        Task<int> GetPendingOrdersCountAsync();
    }
}