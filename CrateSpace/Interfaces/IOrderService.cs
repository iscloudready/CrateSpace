// Interfaces/IOrderService.cs
using CrateSpace.Models.Order;
using CrateSpace.Models.Order.Responses;

namespace CrateSpace.Interfaces
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