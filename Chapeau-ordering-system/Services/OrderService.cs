using System.Transactions;
using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;

namespace Chapeau_ordering_system.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITableRepository _tableRepository;

        public OrderService(IOrderRepository orderRepository,
                            ITableRepository tableRepository)
        {
            _orderRepository = orderRepository;
            _tableRepository = tableRepository;
        }

        public IEnumerable<Order> GetOpenOrders()
        {
            return _orderRepository.GetOpenOrders(); 
        }

        public int StartOrder(int tableId, int employeeId)
        {
            if (tableId <= 0)
                throw new InvalidOperationException("Invalid table.");

            if (employeeId <= 0)
                throw new InvalidOperationException("Invalid employee.");

            using var scope = new TransactionScope(TransactionScopeOption.Required);

            RestaurantTable? table = _tableRepository.GetById(tableId);

            if (table == null)
                throw new InvalidOperationException("Table not found.");

            if (table.Status != TableStatus.Free)
                throw new InvalidOperationException("This table already has an active order. Please load the existing order.");

            Order order = new Order
            {
                Table = table,
                Employee = new Employee { EmployeeId = employeeId },
                OrderTime = DateTime.Now,
                Status = OrderStatus.Open
            };

            int newOrderId = _orderRepository.Add(order);

            _tableRepository.UpdateStatus(tableId, TableStatus.Occupied, newOrderId);

            scope.Complete();

            return newOrderId;
        }

        public void SaveOrder(int orderId, List<OrderItem> items)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("Invalid order.");

            if (!items.Any())
                throw new InvalidOperationException("Order cannot be empty. Add items before sending.");

            using var scope = new TransactionScope(TransactionScopeOption.Required);

            try
            {
                // Items have already been added during AddItemToOrder
                // Stock has already been decreased during AddItemToOrder
                // Just update order status to Submitted so bar/kitchen can see it
                _orderRepository.UpdateOrderStatus(orderId, OrderStatus.Submitted);

                scope.Complete();
            }
            catch (Exception ex)
            {
                scope.Dispose();
                throw new InvalidOperationException($"Failed to save order: {ex.Message}");
            }
        }


        public void IncreaseItemQuantity(int orderItemId, int currentQuantity)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required);

            OrderItem? item = _orderRepository.GetOrderItemById(orderItemId);

            if (item == null || item.MenuItem == null)
                throw new InvalidOperationException("Order item not found.");

            if (item.MenuItem.Stock <= 0)
                throw new InvalidOperationException("This item is out of stock.");

            _orderRepository.UpdateOrderItemQuantity(orderItemId, currentQuantity + 1);
            _orderRepository.DecreaseStock(item.MenuItem.MenuItemId, 1);

            scope.Complete();
        }


        public void DecreaseItemQuantity(int orderItemId, int currentQuantity)
        {
            if (currentQuantity <= 1)
                return;

            using var scope = new TransactionScope(TransactionScopeOption.Required);

            OrderItem? item = _orderRepository.GetOrderItemById(orderItemId);

            if (item == null || item.MenuItem == null)
                throw new InvalidOperationException("Order item not found.");

            _orderRepository.UpdateOrderItemQuantity(orderItemId, currentQuantity - 1);
            _orderRepository.IncreaseStock(item.MenuItem.MenuItemId, 1);

            scope.Complete();
        }

        public void UpdateItemComment(int orderItemId, string? comment)
        {
            _orderRepository.UpdateOrderItemComment(orderItemId, comment);
        }

        public void RemoveItem(int orderItemId)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required);

            OrderItem? item = _orderRepository.GetOrderItemById(orderItemId);

            if (item == null || item.MenuItem == null)
                throw new InvalidOperationException("Order item not found.");

            _orderRepository.RemoveOrderItem(orderItemId);
            _orderRepository.IncreaseStock(item.MenuItem.MenuItemId, item.Quantity);

            scope.Complete();
        }

        public void CancelOrder(int orderId, int tableId)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("No active order found.");

            if (tableId <= 0)
                throw new InvalidOperationException("Invalid table.");

            using var scope = new TransactionScope(TransactionScopeOption.Required);

            List<OrderItem> items = _orderRepository.GetItemsByOrderId(orderId);

            foreach (OrderItem item in items)
            {
                if (item.MenuItem != null)
                {
                    _orderRepository.IncreaseStock(item.MenuItem.MenuItemId, item.Quantity);
                }
            }

            _orderRepository.CancelOrder(orderId);
            _tableRepository.UpdateStatus(tableId, TableStatus.Free, null);

            scope.Complete();
        }

        public List<OrderItem> GetItemsByOrderId(int orderId)
        {
            return _orderRepository.GetItemsByOrderId(orderId);
        }

        public void AddItemToOrder(int orderId, int menuItemId)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("No active order found.");

            using var scope = new TransactionScope(TransactionScopeOption.Required);

            MenuItem? menuItem = _orderRepository.GetMenuItemById(menuItemId);

            if (menuItem == null)
                throw new InvalidOperationException("Menu item not found.");

            if (menuItem.Stock <= 0)
                throw new InvalidOperationException("This item is out of stock.");

            OrderItem? existingItem = _orderRepository.GetOrderItemByOrderAndMenuItem(orderId, menuItemId);

            if (existingItem != null)
            {
                _orderRepository.UpdateOrderItemQuantity(existingItem.OrderItemId, existingItem.Quantity + 1);
                _orderRepository.DecreaseStock(menuItemId, 1);
            }
            else
            {
                OrderItem newItem = new OrderItem
                {
                    MenuItem = menuItem,
                    Quantity = 1,
                    Status = OrderItemStatus.Ordered,
                    OrderTime = DateTime.Now
                };

                _orderRepository.AddOrderItem(orderId, newItem);
                _orderRepository.DecreaseStock(menuItemId, 1);
            }

            scope.Complete();
        }

        public Order? GetOrderById(int orderId)
        {
            return _orderRepository.GetOpenOrders()
                .FirstOrDefault(o => o.OrderId == orderId);
        }
        public Order? GetOrderByTableId(int tableId)
        {
            if (tableId <= 0)
                throw new InvalidOperationException("Invalid table.");

            Order? order = _orderRepository.GetOpenOrders()
                .FirstOrDefault(o => o.Table?.TableId == tableId);

            return order;
        }

        public bool TableHasUnservedItems(int tableId)
        {
            return _orderRepository.TableHasUnservedItems(tableId);
        }

        public void MarkItemServed(int orderItemId)
        {
            _orderRepository.UpdateOrderItemStatus(orderItemId, OrderItemStatus.Served);
        }
    }
}
