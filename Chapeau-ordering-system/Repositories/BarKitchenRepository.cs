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

        // Get running orders (filtered by Bar or Kitchen using MenuItemType)
       public List<Order> GetRunningOrders(MenuItemType menuItemType)
{
    // Validate MenuItemType parameter
    if (menuItemType != MenuItemType.Food && menuItemType != MenuItemType.Drink)
    {
        throw new ArgumentException($"Invalid menu item type: {menuItemType}");
    }

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
            AND oi.Status IN (@OrderedStatus, @BeingPreparedStatus) -- recommendation
            ORDER BY o.OrderTime ASC";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
            command.Parameters.AddWithValue("@OrderedStatus", (int)OrderItemStatus.Ordered);
            command.Parameters.AddWithValue("@BeingPreparedStatus", (int)OrderItemStatus.BeingPrepared);

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

                        orders.Add(orderId, order);
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
                    AND oi.Status IN (@ReadyToBeServedStatus, @ServedStatus)  -- recommendation
                    ORDER BY o.OrderTime ASC";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
                    command.Parameters.AddWithValue("@ReadyToBeServedStatus", (int)OrderItemStatus.ReadyToBeServed);
                    command.Parameters.AddWithValue("@ServedStatus", (int)OrderItemStatus.Served);
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

        // Mark all items in order as ready to be served (regardless of current status)
        public void UpdateAllOrderItemsToReady(int orderId, MenuItemType menuItemType)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "UPDATE OrderItems SET Status = @ReadyStatus WHERE OrderId = @OrderId AND Status IN (1, 2) AND MenuItemId IN (SELECT MenuItemId FROM MenuItems WHERE Type = @MenuItemType)",
                    connection))
                {
                    command.Parameters.AddWithValue("@ReadyStatus", (int)OrderItemStatus.ReadyToBeServed);
                    command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error marking all order items as ready.", ex);
            }
        }
    }
}