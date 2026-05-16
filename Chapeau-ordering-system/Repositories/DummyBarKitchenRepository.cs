using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;

namespace Chapeau_ordering_system.Repositories
{
    public class DummyBarKitchenRepository : IBarKitchenRepository
    {
        // I use a static list so the dummy status changes stay after refresh
        private static List<Order> _orders = CreateDummyOrders();

        public List<Order> GetRunningOrders(MenuItemType menuItemType)
        {
            List<Order> runningOrders = new List<Order>();

            foreach (Order order in _orders)
            {
                Order runningOrder = CreateOrderWithoutItems(order);

                foreach (OrderItem item in order.OrderItems)
                {
                    if (IsRunningItem(item, menuItemType))
                    {
                        runningOrder.OrderItems.Add(item);
                    }
                }

                if (runningOrder.OrderItems.Count > 0)
                {
                    runningOrders.Add(runningOrder);
                }
            }

            return runningOrders;
        }

        public void UpdateOrderItemsStatusForOrder(int orderId, MenuItemType menuItemType, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
            // I update all matching items of one order
            foreach (Order order in _orders)
            {
                if (order.OrderId == orderId)
                {
                    UpdateItemsInOrder(order, menuItemType, oldStatus, newStatus);
                }
            }
        }

        public void UpdateOrderItemStatus(int orderItemId, MenuItemType menuItemType, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
            // I update one matching order item
            foreach (Order order in _orders)
            {
                foreach (OrderItem item in order.OrderItems)
                {
                    if (CanUpdateItem(item, orderItemId, menuItemType, oldStatus))
                    {
                        item.Status = newStatus;
                        SetStatusTime(item, newStatus);
                    }
                }
            }
        }

        private void UpdateItemsInOrder(Order order, MenuItemType menuItemType, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
            foreach (OrderItem item in order.OrderItems)
            {
                if (item.MenuItem?.Type == menuItemType && item.Status == oldStatus)
                {
                    item.Status = newStatus;
                    SetStatusTime(item, newStatus);
                }
            }
        }

        private bool CanUpdateItem(OrderItem item, int orderItemId, MenuItemType menuItemType, OrderItemStatus oldStatus)
        {
            return item.OrderItemId == orderItemId
                && item.MenuItem?.Type == menuItemType
                && item.Status == oldStatus;
        }

        private bool IsRunningItem(OrderItem item, MenuItemType menuItemType)
        {
            return item.MenuItem?.Type == menuItemType
                && (item.Status == OrderItemStatus.Ordered || item.Status == OrderItemStatus.BeingPrepared);
        }

        private void SetStatusTime(OrderItem item, OrderItemStatus newStatus)
        {
            // I save the time when an item starts or becomes ready
            if (newStatus == OrderItemStatus.BeingPrepared)
            {
                item.StartedAt = DateTime.Now;
            }

            if (newStatus == OrderItemStatus.ReadyToBeServed)
            {
                item.ReadyAt = DateTime.Now;
            }
        }

        private Order CreateOrderWithoutItems(Order order)
        {
            return new Order
            {
                OrderId = order.OrderId,
                Table = order.Table,
                Employee = order.Employee,
                OrderTime = order.OrderTime,
                Status = order.Status,
                OrderItems = new List<OrderItem>()
            };
        }

        private static List<Order> CreateDummyOrders()
        {
            List<Order> orders = new List<Order>();

            orders.Add(CreateKitchenOrderOne());
            orders.Add(CreateKitchenOrderTwo());
            orders.Add(CreateBarOrderOne());
            orders.Add(CreateBarOrderTwo());

            return orders;
        }

        private static Order CreateKitchenOrderOne()
        {
            return new Order
            {
                OrderId = 1,
                Table = new RestaurantTable
                {
                    TableId = 1,
                    TableNumber = 1
                },
                Employee = new Employee
                {
                    EmployeeId = 1,
                    FirstName = "Marco",
                    LastName = "Jansen",
                    Role = EmployeeRole.Kitchen
                },
                OrderTime = DateTime.Now.AddMinutes(-20),
                Status = OrderStatus.Open,
                OrderItems = new List<OrderItem>
                {
                    // ✅ FIXED: Fish soup is Ordered (ready to start)
                    new OrderItem
                    {
                        OrderItemId = 1,
                        Quantity = 2,
                        Comment = "No croutons",
                        Status = OrderItemStatus.Ordered,
                        OrderTime = DateTime.Now.AddMinutes(-20),
                        MenuItem = new MenuItem
                        {
                            MenuItemId = 1,
                            Name = "Fish soup",
                            Price = 6.50m,
                            Type = MenuItemType.Food,
                            Course = CourseType.Starter
                        }
                    },
                    // ✅ FIXED: Fried cod is BeingPrepared (already started)
                    new OrderItem
                    {
                        OrderItemId = 2,
                        Quantity = 1,
                        Comment = "Extra sauce",
                        Status = OrderItemStatus.BeingPrepared,
                        OrderTime = DateTime.Now.AddMinutes(-20),
                        MenuItem = new MenuItem
                        {
                            MenuItemId = 2,
                            Name = "Fried cod",
                            Price = 17.50m,
                            Type = MenuItemType.Food,
                            Course = CourseType.Main
                        }
                    }
                }
            };
        }

        private static Order CreateKitchenOrderTwo()
        {
            return new Order
            {
                OrderId = 2,
                Table = new RestaurantTable
                {
                    TableId = 2,
                    TableNumber = 2
                },
                Employee = new Employee
                {
                    EmployeeId = 1,
                    FirstName = "Marco",
                    LastName = "Jansen",
                    Role = EmployeeRole.Kitchen
                },
                OrderTime = DateTime.Now.AddMinutes(-10),
                Status = OrderStatus.Open,
                OrderItems = new List<OrderItem>
                {
                    // ✅ FIXED: Fried tenderloin is Ordered (ready to start)
                    new OrderItem
                    {
                        OrderItemId = 3,
                        Quantity = 1,
                        Comment = "Medium rare",
                        Status = OrderItemStatus.Ordered,
                        OrderTime = DateTime.Now.AddMinutes(-10),
                        MenuItem = new MenuItem
                        {
                            MenuItemId = 3,
                            Name = "Fried tenderloin",
                            Price = 22.50m,
                            Type = MenuItemType.Food,
                            Course = CourseType.Main
                        }
                    },
                    // ✅ FIXED: White chocolate cake is BeingPrepared (already started)
                    new OrderItem
                    {
                        OrderItemId = 4,
                        Quantity = 2,
                        Comment = null,
                        Status = OrderItemStatus.BeingPrepared,
                        OrderTime = DateTime.Now.AddMinutes(-10),
                        MenuItem = new MenuItem
                        {
                            MenuItemId = 4,
                            Name = "White chocolate cake",
                            Price = 5.50m,
                            Type = MenuItemType.Food,
                            Course = CourseType.Dessert
                        }
                    }
                }
            };
        }

        private static Order CreateBarOrderOne()
        {
            return new Order
            {
                OrderId = 3,
                Table = new RestaurantTable
                {
                    TableId = 3,
                    TableNumber = 3
                },
                Employee = new Employee
                {
                    EmployeeId = 2,
                    FirstName = "Bar",
                    LastName = "Employee",
                    Role = EmployeeRole.Bar
                },
                OrderTime = DateTime.Now.AddMinutes(-15),
                Status = OrderStatus.Open,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        OrderItemId = 5,
                        Quantity = 2,
                        Comment = "With ice",
                        Status = OrderItemStatus.Ordered,
                        OrderTime = DateTime.Now.AddMinutes(-15),
                        MenuItem = new MenuItem
                        {
                            MenuItemId = 5,
                            Name = "Coca Cola",
                            Price = 2.50m,
                            Type = MenuItemType.Drink,
                            Course = CourseType.None
                        }
                    }
                }
            };
        }

        private static Order CreateBarOrderTwo()
        {
            return new Order
            {
                OrderId = 4,
                Table = new RestaurantTable
                {
                    TableId = 4,
                    TableNumber = 4
                },
                Employee = new Employee
                {
                    EmployeeId = 2,
                    FirstName = "Bar",
                    LastName = "Employee",
                    Role = EmployeeRole.Bar
                },
                OrderTime = DateTime.Now.AddMinutes(-5),
                Status = OrderStatus.Open,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        OrderItemId = 6,
                        Quantity = 1,
                        Comment = null,
                        Status = OrderItemStatus.BeingPrepared,
                        OrderTime = DateTime.Now.AddMinutes(-5),
                        MenuItem = new MenuItem
                        {
                            MenuItemId = 6,
                            Name = "Duvel",
                            Price = 4.50m,
                            Type = MenuItemType.Drink,
                            Course = CourseType.None
                        }
                    }
                }
            };
        }

        // School project CRUD methods (not used in Bar/Kitchen, but required by interface)
        public List<Order> GetAll()
        {
            return _orders;
        }

        public Order? GetById(int orderId)
        {
            foreach (Order order in _orders)
            {
                if (order.OrderId == orderId)
                {
                    return order;
                }
            }
            return null;
        }

        public void Add(Order order)
        {
            // Not implemented in dummy repository
        }

        public void Update(Order order)
        {
            // Not implemented in dummy repository
        }

        public void Delete(int orderId)
        {
            // Not implemented in dummy repository
        }

        public void UpdateStatus(int orderId, string status)
        {
            // Not implemented in dummy repository
        }
    }
}