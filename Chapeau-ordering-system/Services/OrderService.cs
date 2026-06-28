using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;
using System.Transactions;

namespace Chapeau_ordering_system.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITableRepository _tableRepository;

        public OrderService(IOrderRepository orderRepository, ITableRepository tableRepository)
        {
            _orderRepository = orderRepository;
            _tableRepository = tableRepository;
        }

        public IEnumerable<Order> GetOpenOrders() => _orderRepository.GetOpenOrders();
        public Order? GetOrderById(int orderId)   => _orderRepository.GetOrderById(orderId);
        public List<OrderItem> GetItemsByOrderId(int orderId) => _orderRepository.GetItemsByOrderId(orderId);
        public List<OrderItem> GetSentItemsByTableId(int tableId) => _orderRepository.GetSentItemsByTableId(tableId);
        public bool TableHasUnservedItems(int tableId) => _orderRepository.TableHasUnservedItems(tableId);
        public void MarkItemServed(int orderItemId)        => _orderRepository.UpdateOrderItemStatus(orderItemId, OrderItemStatus.Served);
        public void MarkItemBeingPrepared(int orderItemId) => _orderRepository.UpdateOrderItemStatus(orderItemId, OrderItemStatus.BeingPrepared);
        public void UpdateItemComment(int orderItemId, string? comment) => _orderRepository.UpdateOrderItemComment(orderItemId, comment);
        public string GetItemNameById(int menuItemId) => _orderRepository.GetMenuItemById(menuItemId)?.Name ?? "Item";
        public string GetItemNameByOrderItemId(int orderItemId) => _orderRepository.GetOrderItemById(orderItemId)?.MenuItem?.Name ?? "Item";
        public OrderItem? GetOrderItemById(int orderItemId) => _orderRepository.GetOrderItemById(orderItemId);

        public Order? GetOrderByTableId(int tableId)
        {
            if (tableId <= 0) throw new InvalidOperationException("Invalid table.");
            return _orderRepository.GetOrderByTableId(tableId);
        }

        // ── GetOrCreateOrder ─────────────────────────────────────────────────────

        public int GetOrCreateOrder(int tableId, int employeeId)
        {
            var existing = _orderRepository.GetOrderByTableId(tableId);
            if (existing != null) return existing.OrderId;
            return StartOrder(tableId, employeeId);
        }

        // ── StartOrder ───────────────────────────────────────────────────────────

        public int StartOrder(int tableId, int employeeId)
        {
            ValidateStartOrderParameters(tableId, employeeId);
            using var scope = new TransactionScope(TransactionScopeOption.Required);
            var table   = GetTableOrThrow(tableId);
            EnsureNoActiveOrder(tableId);
            int orderId = CreateAndSaveOrder(table, employeeId);
            _tableRepository.UpdateStatus(tableId, TableStatus.Occupied, orderId);
            scope.Complete();
            return orderId;
        }

        private static void ValidateStartOrderParameters(int tableId, int employeeId)
        {
            if (tableId   <= 0) throw new InvalidOperationException("Invalid table.");
            if (employeeId <= 0) throw new InvalidOperationException("Invalid employee.");
        }

        private RestaurantTable GetTableOrThrow(int tableId) =>
            _tableRepository.GetById(tableId) ?? throw new InvalidOperationException("Table not found.");

        private void EnsureNoActiveOrder(int tableId)
        {
            if (_orderRepository.HasAnyActiveOrder(tableId))
                throw new InvalidOperationException("This table already has an active order. Please load the existing order.");
        }

        private int CreateAndSaveOrder(RestaurantTable table, int employeeId)
        {
            var order = new Order { Table = table, Employee = new Employee { EmployeeId = employeeId }, OrderTime = DateTime.Now, Status = OrderStatus.Open };
            return _orderRepository.Add(order);
        }

        // ── SaveOrder ────────────────────────────────────────────────────────────

        public void SaveOrder(int orderId)
        {
            if (orderId <= 0) throw new InvalidOperationException("Invalid order.");
            using var scope = new TransactionScope(TransactionScopeOption.Required);

            var orderItems = _orderRepository.GetItemsByOrderId(orderId);
            if (!orderItems.Any())
                throw new InvalidOperationException("Order cannot be empty. Add items before sending.");
            foreach (var item in orderItems.Where(i => i.MenuItem != null))
            {
                _orderRepository.DecreaseStock(item.MenuItem!.MenuItemId, item.Quantity);
            }

            _orderRepository.UpdateOrderStatus(orderId, OrderStatus.Submitted);
            scope.Complete();
        }

        // ── AddItemToOrder ───────────────────────────────────────────────────────

        public void AddItemToOrder(int orderId, int menuItemId)
        {
            if (orderId <= 0) throw new InvalidOperationException("No active order found.");
            using var scope = new TransactionScope(TransactionScopeOption.Required);
            var menuItem = GetMenuItemOrThrowIfUnavailable(menuItemId);
            AddOrIncrementOrderItem(orderId, menuItem);
            scope.Complete();
        }

        private MenuItem GetMenuItemOrThrowIfUnavailable(int menuItemId)
        {
            var item = _orderRepository.GetMenuItemById(menuItemId) ?? throw new InvalidOperationException("Menu item not found.");
            if (item.IsOutOfStock) throw new InvalidOperationException("This item is out of stock.");
            return item;
        }

        private void AddOrIncrementOrderItem(int orderId, MenuItem menuItem)
        {
            var existing = _orderRepository.GetOrderItemByOrderAndMenuItem(orderId, menuItem.MenuItemId);
            if (existing != null)
                _orderRepository.UpdateOrderItemQuantity(existing.OrderItemId, existing.Quantity + 1);
            else
                _orderRepository.AddOrderItem(orderId, CreateOrderItem(menuItem));
            // Stock is NOT decreased here - only on SaveOrder (when order is submitted)
        }

        private static OrderItem CreateOrderItem(MenuItem menuItem) => new()
        {
            MenuItem  = menuItem,
            Quantity  = 1,
            Status    = OrderItemStatus.Ordered,
            OrderTime = DateTime.Now
        };

        // ── IncreaseItemQuantity ─────────────────────────────────────────────────

        public void IncreaseItemQuantity(int orderItemId)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required);
            var item = GetOrderItemOrThrow(orderItemId);
            if (item.MenuItem!.IsOutOfStock) throw new InvalidOperationException("This item is out of stock.");
            // Fetch real quantity from DB instead of trusting client value
            _orderRepository.UpdateOrderItemQuantity(orderItemId, item.Quantity + 1);
            // Stock is NOT decreased here - only on SaveOrder (when order is submitted)
            scope.Complete();
        }

        // ── DecreaseItemQuantity ─────────────────────────────────────────────────

        public void DecreaseItemQuantity(int orderItemId)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required);
            var item = GetOrderItemOrThrow(orderItemId);
            if (item.Quantity <= 1) return;  // Can't decrease below 1
            // Fetch real quantity from DB instead of trusting client value
            _orderRepository.UpdateOrderItemQuantity(orderItemId, item.Quantity - 1);
            // Stock is NOT increased here (it was never decreased during draft phase)
            scope.Complete();
        }

        // ── RemoveItem ───────────────────────────────────────────────────────────

        public void RemoveItem(int orderItemId)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required);
            var item = GetOrderItemOrThrow(orderItemId);
            _orderRepository.RemoveOrderItem(orderItemId);
            // Stock is NOT increased here - it was never decreased during draft phase
            scope.Complete();
        }

        // ── CancelOrder ──────────────────────────────────────────────────────────

        public void CancelOrder(int orderId, int tableId)
        {
            ValidateCancelInputs(orderId, tableId);
            // Fetch the real order from DB to validate it belongs to the claimed tableId
            var order = _orderRepository.GetOrderById(orderId) 
                ?? throw new InvalidOperationException("Order not found.");
            if (order.Table?.TableId != tableId)
                throw new InvalidOperationException("Order does not belong to this table. Cancellation denied.");

            using var scope = new TransactionScope(TransactionScopeOption.Required);
            // Only restore stock if order was already submitted (status was Submitted)
            if (order.Status == OrderStatus.Submitted)
            {
                RestoreStockForOrder(orderId);
            }
            _orderRepository.CancelOrder(orderId);
            _tableRepository.UpdateStatus(tableId, TableStatus.Free, null);
            scope.Complete();
        }

        private static void ValidateCancelInputs(int orderId, int tableId)
        {
            if (orderId <= 0) throw new InvalidOperationException("No active order found.");
            if (tableId <= 0) throw new InvalidOperationException("Invalid table.");
        }

        private void RestoreStockForOrder(int orderId)
        {
            foreach (var item in _orderRepository.GetItemsByOrderId(orderId))
                if (item.MenuItem != null)
                    _orderRepository.IncreaseStock(item.MenuItem.MenuItemId, item.Quantity);
        }

        // ── Shared guard ─────────────────────────────────────────────────────────

        private OrderItem GetOrderItemOrThrow(int orderItemId)
        {
            var item = _orderRepository.GetOrderItemById(orderItemId);
            if (item == null || item.MenuItem == null) throw new InvalidOperationException("Order item not found.");
            return item;
        }
    }
}
