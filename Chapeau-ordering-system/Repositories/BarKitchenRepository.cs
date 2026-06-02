using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class BarKitchenRepository : IBarKitchenRepository
    {
        private readonly string _connectionString;

        public BarKitchenRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase")
                ?? throw new ArgumentNullException(
                    nameof(configuration),
                    "Connection string 'ChapeauDatabase' not found in configuration.");
        }

        // ============================================
        // MAPPING HELPER METHODS
        // ============================================

        private Order MapOrder(SqlDataReader reader)
        {
            Order order = new Order();
            order.OrderId = (int)reader["OrderId"];
            order.OrderTime = (DateTime)reader["OrderTime"];
            order.Status = (OrderStatus)(int)reader["Status"];
            return order;
        }

        private RestaurantTable MapRestaurantTable(SqlDataReader reader)
        {
            RestaurantTable table = new RestaurantTable();
            table.TableId = (int)reader["TableId"];
            table.TableNumber = (string)reader["TableNumber"];
            return table;
        }

        private Employee MapEmployee(SqlDataReader reader)
        {
            Employee employee = new Employee();
            employee.EmployeeId = (int)reader["EmployeeId"];
            employee.FirstName = (string)reader["FirstName"];
            employee.LastName = (string)reader["LastName"];
            employee.Role = (EmployeeRole)(int)reader["Role"];
            return employee;
        }

        private OrderItem MapOrderItem(SqlDataReader reader)
        {
            OrderItem orderItem = new OrderItem();
            orderItem.OrderItemId = (int)reader["OrderItemId"];
            orderItem.MenuItem = new MenuItem();
            orderItem.MenuItem.MenuItemId = (int)reader["MenuItemId"];
            orderItem.MenuItem.Name = (string)reader["MenuItemName"];
            orderItem.MenuItem.Price = (decimal)reader["Price"];
            orderItem.MenuItem.Type = (MenuItemType)(int)reader["Type"];
            orderItem.MenuItem.Course = (CourseType)(int)reader["Course"];
            orderItem.Quantity = (int)reader["Quantity"];
            orderItem.Comment = reader["Comment"] == DBNull.Value ? null : (string)reader["Comment"];
            orderItem.Status = (OrderItemStatus)(int)reader["OrderItemStatus"];
            orderItem.OrderTime = (DateTime)reader["OrderItemTime"];
            return orderItem;
        }

        // ============================================
        // CRUD OPERATIONS
        // ============================================

        public List<Order> GetAll()
        {
            List<Order> orders = new List<Order>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SELECT * FROM Orders", connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(MapOrder(reader));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error fetching all orders.", ex);
            }
            return orders;
        }

        // Get single order by OrderId
        public Order? GetById(int orderId)
        {
            Order? order = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SELECT * FROM Orders WHERE OrderId = @OrderId", connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = MapOrder(reader);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error fetching order by ID.", ex);
            }
            return order;
        }
                // Add new order
        public void Add(Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Orders (TableId, EmployeeId, Status, OrderTime) VALUES (@TableId, @EmployeeId, @Status, @OrderTime)",
                    connection))
                {
                    command.Parameters.AddWithValue("@TableId", order.Table?.TableId ?? 0);
                    command.Parameters.AddWithValue("@EmployeeId", order.Employee?.EmployeeId ?? 0);
                    command.Parameters.AddWithValue("@Status", (int)order.Status);
                    command.Parameters.AddWithValue("@OrderTime", order.OrderTime);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error adding order.", ex);
            }
        }
        // Update order
        public void Update(Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "UPDATE Orders SET TableId = @TableId, Status = @Status WHERE OrderId = @OrderId",
                    connection))
                {
                    command.Parameters.AddWithValue("@OrderId", order.OrderId);
                    command.Parameters.AddWithValue("@TableId", order.Table?.TableId ?? 0);
                    command.Parameters.AddWithValue("@Status", (int)order.Status);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error updating order.", ex);
            }
        }

        // Delete order
        public void Delete(int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("DELETE FROM Orders WHERE OrderId = @OrderId",
                    connection))
                    
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error deleting order.", ex);
            }
        }

        // Get running orders (filtered by Bar or Kitchen using MenuItemType)
        public List<Order> GetRunningOrders(MenuItemType menuItemType)
        {
            // Validate MenuItemType parameter
            if (menuItemType != MenuItemType.Food && menuItemType != MenuItemType.Drink)
                throw new ArgumentException($"Invalid menu item type: {menuItemType}");

            Dictionary<int, Order> orders = new Dictionary<int, Order>();
            try
            {
                string query = @"
                    SELECT 
                        o.OrderId, o.TableId, t.TableNumber, o.EmployeeId,
                        e.FirstName, e.LastName, e.Role, o.OrderTime, o.Status,
                        oi.OrderItemId, oi.MenuItemId, oi.Quantity, oi.Comment,
                        oi.Status AS OrderItemStatus, oi.OrderTime AS OrderItemTime,
                        mi.Name AS MenuItemName, mi.Price, mi.Type, mi.Course
                    FROM  Orders o
                    INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
                    INNER JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
                    INNER JOIN Tables t ON o.TableId = t.TableId
                    INNER JOIN Employees e ON o.EmployeeId = e.EmployeeId
                    WHERE mi.Type = @MenuItemType
                    AND oi.Status IN (1, 2)
                    ORDER BY o.OrderTime ASC";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = (int)reader["OrderId"];
                            if (!orders.ContainsKey(orderId))
                            {
                                Order order = MapOrder(reader);
                                order.Table = MapRestaurantTable(reader);
                                order.Employee = MapEmployee(reader);
                                order.OrderItems = new List<OrderItem>();
                                orders[orderId] = order;
                            }

                            OrderItem orderItem = MapOrderItem(reader);
                            orders[orderId].OrderItems.Add(orderItem);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error fetching running orders.", ex);
            }
            return orders.Values.ToList();
        }

        // Get finished orders today (filtered by Bar or Kitchen using MenuItemType)
        public List<Order> GetFinishedOrdersToday(MenuItemType menuItemType)
        {
            // Validate MenuItemType parameter
            if (menuItemType != MenuItemType.Food && menuItemType != MenuItemType.Drink)
                throw new ArgumentException($"Invalid menu item type: {menuItemType}");

            Dictionary<int, Order> orders = new Dictionary<int, Order>();
            try
            {
                string query = @"
                    SELECT 
                        o.OrderId, o.TableId, t.TableNumber, o.EmployeeId,
                        e.FirstName, e.LastName, e.Role, o.OrderTime, o.Status,
                        oi.OrderItemId, oi.MenuItemId, oi.Quantity, oi.Comment,
                        oi.Status AS OrderItemStatus, oi.OrderTime AS OrderItemTime,
                        mi.Name AS MenuItemName, mi.Price, mi.Type, mi.Course
                    FROM Orders o
                    INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
                    INNER JOIN MenuItems mi ON oi.MenuItemId = mi.MenuItemId
                    INNER JOIN Tables t ON o.TableId = t.TableId
                    INNER JOIN Employees e ON o.EmployeeId = e.EmployeeId
                    WHERE mi.Type = @MenuItemType
                    AND oi.Status = 3
                    AND CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                    ORDER BY o.OrderTime ASC";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = (int)reader["OrderId"];
                            if (!orders.ContainsKey(orderId))
                            {
                                Order order = MapOrder(reader);
                                order.Table = MapRestaurantTable(reader);
                                order.Employee = MapEmployee(reader);
                                order.OrderItems = new List<OrderItem>();
                                orders[orderId] = order;
                            }

                            OrderItem orderItem = MapOrderItem(reader);
                            orders[orderId].OrderItems.Add(orderItem);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error fetching finished orders today.", ex);
            }
            return orders.Values.ToList();
        }

        // Update order status
        public void UpdateStatus(int orderId, string status)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId",
                    connection))
                {
                    if (Enum.TryParse<OrderStatus>(status, out var statusEnum))
                    {
                        command.Parameters.AddWithValue("@Status", (int)statusEnum);
                    }
                    else
                    {
                        throw new Exception("Invalid order status: " + status);
                    }
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error updating order status.", ex);
            }
        }

        // Update all items in order with specific status (Bar/Kitchen complex method)
        public void UpdateOrderItemsStatusForOrder(int orderId, MenuItemType menuItemType, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "UPDATE OrderItems SET Status = @NewStatus WHERE OrderId = @OrderId AND Status = @OldStatus AND MenuItemId IN (SELECT MenuItemId FROM MenuItems WHERE Type = @MenuItemType)",
                    connection))
                {
                    command.Parameters.AddWithValue("@NewStatus", (int)newStatus);
                    command.Parameters.AddWithValue("@OldStatus", (int)oldStatus);
                    command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error updating order items status.", ex);
            }
        }

        // Update single item with role filtering (Bar/Kitchen complex method)
        public void UpdateOrderItemStatus(int orderItemId, MenuItemType menuItemType, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
            // Validate MenuItemType parameter
            if (menuItemType != MenuItemType.Food && menuItemType != MenuItemType.Drink)
                throw new ArgumentException($"Invalid menu item type: {menuItemType}");

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "UPDATE OrderItems SET Status = @NewStatus WHERE OrderItemId = @OrderItemId AND Status = @OldStatus AND MenuItemId IN (SELECT MenuItemId FROM MenuItems WHERE Type = @MenuItemType)",
                    connection))
                {
                    command.Parameters.AddWithValue("@NewStatus", (int)newStatus);
                    command.Parameters.AddWithValue("@OldStatus", (int)oldStatus);
                    command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
                    command.Parameters.AddWithValue("@OrderItemId", orderItemId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error updating order item status.", ex);
            }
        }

        // Update all items in a course (Kitchen only, Food items)
        public void UpdateCourseStatus(int orderId, CourseType courseType, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "UPDATE OrderItems SET Status = @NewStatus WHERE OrderId = @OrderId AND Status = @OldStatus AND MenuItemId IN (SELECT MenuItemId FROM MenuItems WHERE Type = 1 AND Course = @Course)",
                    connection))
                {
                    command.Parameters.AddWithValue("@NewStatus", (int)newStatus);
                    command.Parameters.AddWithValue("@OldStatus", (int)oldStatus);
                    command.Parameters.AddWithValue("@Course", (int)courseType);
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error updating course status.", ex);
            }
        }
    }
}