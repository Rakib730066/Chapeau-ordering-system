using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class BarKitchenRepository : IBarKitchenRepository
    {
        private readonly string? _connectionString;

        public BarKitchenRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase");
        }

        // Get all orders from database
        public List<Order> GetAll()
        {
            List<Order> orders = new List<Order>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SELECT * FROM Orders", connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Order order = new Order();
                        order.OrderId = (int)reader["OrderId"];
                        order.OrderTime = (DateTime)reader["OrderTime"];
                        order.Status = (OrderStatus)(int)reader["Status"];
                        orders.Add(order);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all orders: " + ex.Message);
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
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        order = new Order();
                        order.OrderId = (int)reader["OrderId"];
                        order.OrderTime = (DateTime)reader["OrderTime"];
                        order.Status = (OrderStatus)(int)reader["Status"];
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching order by ID: " + ex.Message);
            }
            return order;
        }

        // Get running orders (filtered by Bar or Kitchen using MenuItemType)
        public List<Order> GetRunningOrders(MenuItemType menuItemType)
        {
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
                    AND oi.Status IN (1, 2)
                    ORDER BY o.OrderTime ASC";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MenuItemType", (int)menuItemType);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int orderId = (int)reader["OrderId"];
                        if (!orders.ContainsKey(orderId))
                        {
                            Order order = new Order();
                            order.OrderId = orderId;
                            order.Table = new RestaurantTable();
                            order.Table.TableId = (int)reader["TableId"];
                            order.Table.TableNumber = (int)reader["TableNumber"];
                            order.Employee = new Employee();
                            order.Employee.EmployeeId = (int)reader["EmployeeId"];
                            order.Employee.FirstName = (string)reader["FirstName"];
                            order.Employee.LastName = (string)reader["LastName"];
                            order.Employee.Role = (EmployeeRole)(int)reader["Role"];
                            order.OrderTime = (DateTime)reader["OrderTime"];
                            order.Status = (OrderStatus)(int)reader["Status"];
                            order.OrderItems = new List<OrderItem>();
                            orders[orderId] = order;
                        }

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
                        orders[orderId].OrderItems.Add(orderItem);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching running orders: " + ex.Message);
            }
            return orders.Values.ToList();
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
            catch (Exception ex)
            {
                throw new Exception("Error adding order: " + ex.Message);
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
            catch (Exception ex)
            {
                throw new Exception("Error updating order: " + ex.Message);
            }
        }

        // Delete order
        public void Delete(int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(
                    "DELETE FROM Orders WHERE OrderId = @OrderId",
                    connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting order: " + ex.Message);
            }
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
            catch (Exception ex)
            {
                throw new Exception("Error updating order status: " + ex.Message);
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
            catch (Exception ex)
            {
                throw new Exception("Error updating order items status: " + ex.Message);
            }
        }

        // Update single item with role filtering (Bar/Kitchen complex method)
        public void UpdateOrderItemStatus(int orderItemId, MenuItemType menuItemType, OrderItemStatus oldStatus, OrderItemStatus newStatus)
        {
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
            catch (Exception ex)
            {
                throw new Exception("Error updating order item status: " + ex.Message);
            }
        }
    }
}