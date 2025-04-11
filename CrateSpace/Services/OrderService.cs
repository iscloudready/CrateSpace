// Services/OrderService.cs
using InsightOps.Monolith.Interfaces;
using InsightOps.Monolith.Models.Order;
using InsightOps.Monolith.Models.Order.Responses;

namespace InsightOps.Monolith.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IInventoryService inventoryService,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all orders");
                return await _orderRepository.GetAllOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all orders");
                throw;
            }
        }

        public async Task<OrderResponse> PlaceOrderAsync(CreateOrderDto orderDto)
        {
            try
            {
                // Check item availability
                bool isAvailable = await _inventoryService.CheckAvailabilityAsync(orderDto.ItemName, orderDto.Quantity);
                if (!isAvailable)
                {
                    return new OrderResponse
                    {
                        Success = false,
                        Message = $"Insufficient inventory for item: {orderDto.ItemName}"
                    };
                }

                // Get item price
                var item = await _inventoryService.GetItemByNameAsync(orderDto.ItemName);
                if (item == null)
                {
                    return new OrderResponse
                    {
                        Success = false,
                        Message = $"Item not found: {orderDto.ItemName}"
                    };
                }

                // Create order
                var order = new Order
                {
                    ItemName = orderDto.ItemName,
                    Quantity = orderDto.Quantity,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalPrice = item.Price * orderDto.Quantity
                };

                // Reserve stock (update inventory)
                bool stockReserved = await _inventoryService.ReserveStockAsync(orderDto.ItemName, orderDto.Quantity);
                if (!stockReserved)
                {
                    return new OrderResponse
                    {
                        Success = false,
                        Message = "Failed to reserve stock for order"
                    };
                }

                // Save the order
                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                return new OrderResponse
                {
                    OrderId = createdOrder.Id,
                    ItemName = createdOrder.ItemName,
                    Quantity = createdOrder.Quantity,
                    TotalPrice = createdOrder.TotalPrice,
                    Status = createdOrder.Status,
                    OrderDate = createdOrder.OrderDate,
                    Success = true,
                    Message = "Order placed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order for item {ItemName}", orderDto.ItemName);
                return new OrderResponse
                {
                    Success = false,
                    Message = "Failed to place order: " + ex.Message
                };
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Cancelling order {OrderId}", orderId);

                // Get the order
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null || order.Status == "Cancelled")
                    return false;

                // Check if we need to return inventory (only if status is not Delivered)
                if (order.Status != "Delivered")
                {
                    // Get current item
                    var item = await _inventoryService.GetItemByNameAsync(order.ItemName);
                    if (item != null)
                    {
                        // Return items to inventory
                        await _inventoryService.UpdateStockAsync(item.Id, item.Quantity + order.Quantity);
                    }
                }

                return await _orderRepository.UpdateOrderStatusAsync(orderId, "Cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<OrderStatusResponse> GetOrderStatusAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    throw new KeyNotFoundException($"Order with ID {orderId} not found");
                }

                return new OrderStatusResponse
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    ItemName = order.ItemName,
                    Quantity = order.Quantity,
                    LastUpdated = order.OrderDate,
                    TotalPrice = order.TotalPrice,
                    Notes = GetOrderNotes(order)
                };
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving status for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)
        {
            try
            {
                _logger.LogInformation("Retrieving {Count} recent orders", count);
                var allOrders = await _orderRepository.GetAllOrdersAsync();
                return allOrders.Take(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent orders");
                throw;
            }
        }

        public async Task<int> GetPendingOrdersCountAsync()
        {
            try
            {
                _logger.LogInformation("Counting pending orders");
                var pendingOrders = await _orderRepository.GetOrdersByStatusAsync("Pending");
                return pendingOrders.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting pending orders");
                throw;
            }
        }

        private string GetOrderNotes(Order order)
        {
            return order.Status switch
            {
                "Pending" => "Order is being processed",
                "Confirmed" => "Order has been confirmed and is being prepared",
                "Shipped" => "Order has been shipped",
                "Delivered" => "Order has been delivered",
                "Cancelled" => "Order has been cancelled",
                _ => string.Empty
            };
        }
    }
}