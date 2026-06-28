using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase")!;
        }

        public IEnumerable<Order> GetOpenOrders()
        {
            var orders = new Dictionary<int, Order>();

            const string query = @"
                SELECT
                    o.OrderId, o.OrderTime, o.Status           AS OrderStatus,
                    t.TableId, t.TableNumber, t.NumberOfSeats,
                    t.Status                                   AS TableStatus,
                    t.OccupiedSince,
                    e.EmployeeId, e.FirstName, e.LastName,
                    oi.OrderItemId, oi.Quantity, oi.Comment,
                    oi.Status                                  AS ItemStatus,
                    oi.OrderTime                               AS ItemOrderTime,
                    oi.StartedAt, oi.ReadyAt,
                    mi.MenuItemId, mi.Name AS MenuItemName,
                    mi.Price, mi.Type AS MenuItemType, mi.Course
                FROM dbo.Orders o
                INNER JOIN dbo.Tables    t  ON t.TableId    = o.TableId
                INNER JOIN dbo.Employees e  ON e.EmployeeId = o.EmployeeId
                LEFT  JOIN dbo.OrderItems oi ON oi.OrderId   = o.OrderId
                LEFT  JOIN dbo.MenuItems  mi ON mi.MenuItemId = oi.MenuItemId
                WHERE o.Status IN (1, 2)
                AND   CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY o.OrderId, oi.OrderItemId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int orderId = reader.GetInt32(reader.GetOrdinal("OrderId"));

                    if (!orders.TryGetValue(orderId, out Order? order))
                    {
                        order = new Order
                        {
                            OrderId = orderId,
                            OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                            Status = (OrderStatus)reader.GetInt32(reader.GetOrdinal("OrderStatus")),
                            Table = new RestaurantTable
                            {
                                TableId = reader.GetInt32(reader.GetOrdinal("TableId")),
                                TableNumber = reader.GetString(reader.GetOrdinal("TableNumber")),
                                NumberOfSeats = reader.GetInt32(reader.GetOrdinal("NumberOfSeats")),
                                Status = (TableStatus)reader.GetInt32(reader.GetOrdinal("TableStatus")),
                                OccupiedSince = reader.IsDBNull(reader.GetOrdinal("OccupiedSince"))
                                                ? null
                                                : reader.GetDateTime(reader.GetOrdinal("OccupiedSince"))
                            },
                            Employee = new Employee
                            {
                                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName"))
                            }
                        };
                        orders[orderId] = order;
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("OrderItemId")))
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Comment = reader.IsDBNull(reader.GetOrdinal("Comment"))
                                          ? null
                                          : reader.GetString(reader.GetOrdinal("Comment")),
                            Status = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("ItemStatus")),
                            OrderTime = reader.GetDateTime(reader.GetOrdinal("ItemOrderTime")),
                            StartedAt = reader.IsDBNull(reader.GetOrdinal("StartedAt"))
                                          ? default
                                          : reader.GetDateTime(reader.GetOrdinal("StartedAt")),
                            ReadyAt = reader.IsDBNull(reader.GetOrdinal("ReadyAt"))
                                          ? default
                                          : reader.GetDateTime(reader.GetOrdinal("ReadyAt")),
                            MenuItem = reader.IsDBNull(reader.GetOrdinal("MenuItemId"))
                                       ? null
                                       : new MenuItem
                                       {
                                           MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                                           Name = reader.GetString(reader.GetOrdinal("MenuItemName")),
                                           Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                           Type = (MenuItemType)reader.GetInt32(reader.GetOrdinal("MenuItemType")),
                                           Course = (CourseType)reader.GetInt32(reader.GetOrdinal("Course"))
                                       }
                        });
                    }
                }

                return orders.Values;
            }
            catch (SqlException)
            {
                return orders.Values;
            }
        }


        public Order? GetOrderById(int orderId)
        {
            const string query = @"
                SELECT
                    o.OrderId, o.OrderTime, o.Status          AS OrderStatus,
                    t.TableId, t.TableNumber, t.NumberOfSeats,
                    t.Status                                  AS TableStatus,
                    t.OccupiedSince,
                    e.EmployeeId, e.FirstName, e.LastName
                FROM dbo.Orders o
                INNER JOIN dbo.Tables    t ON t.TableId    = o.TableId
                INNER JOIN dbo.Employees e ON e.EmployeeId = o.EmployeeId
                WHERE o.OrderId = @OrderId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (!reader.Read()) return null;

                return new Order
                {
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                    Status = (OrderStatus)reader.GetInt32(reader.GetOrdinal("OrderStatus")),
                    Table = new RestaurantTable
                    {
                        TableId = reader.GetInt32(reader.GetOrdinal("TableId")),
                        TableNumber = reader.GetString(reader.GetOrdinal("TableNumber")),
                        NumberOfSeats = reader.GetInt32(reader.GetOrdinal("NumberOfSeats")),
                        Status = (TableStatus)reader.GetInt32(reader.GetOrdinal("TableStatus")),
                        OccupiedSince = reader.IsDBNull(reader.GetOrdinal("OccupiedSince"))
                                        ? null
                                        : reader.GetDateTime(reader.GetOrdinal("OccupiedSince"))
                    },
                    Employee = new Employee
                    {
                        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName"))
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching order by id: " + ex.Message);
            }
        }

        public Order? GetOrderByTableId(int tableId)
        {
            const string query = @"
                SELECT
                    o.OrderId, o.OrderTime, o.Status          AS OrderStatus,
                    t.TableId, t.TableNumber, t.NumberOfSeats,
                    t.Status                                  AS TableStatus,
                    t.OccupiedSince,
                    e.EmployeeId, e.FirstName, e.LastName
                FROM dbo.Orders o
                INNER JOIN dbo.Tables    t ON t.TableId    = o.TableId
                INNER JOIN dbo.Employees e ON e.EmployeeId = o.EmployeeId
                WHERE o.TableId = @TableId
                AND   o.Status = 1
                AND   CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY o.OrderTime DESC";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TableId", tableId);
                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (!reader.Read()) return null;

                return new Order
                {
                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                    OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                    Status = (OrderStatus)reader.GetInt32(reader.GetOrdinal("OrderStatus")),
                    Table = new RestaurantTable
                    {
                        TableId = reader.GetInt32(reader.GetOrdinal("TableId")),
                        TableNumber = reader.GetString(reader.GetOrdinal("TableNumber")),
                        NumberOfSeats = reader.GetInt32(reader.GetOrdinal("NumberOfSeats")),
                        Status = (TableStatus)reader.GetInt32(reader.GetOrdinal("TableStatus")),
                        OccupiedSince = reader.IsDBNull(reader.GetOrdinal("OccupiedSince"))
                                        ? null
                                        : reader.GetDateTime(reader.GetOrdinal("OccupiedSince"))
                    },
                    Employee = new Employee
                    {
                        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName"))
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching order by table id: " + ex.Message);
            }
        }

        public int Add(Order order)
        {
            const string query = @"
        INSERT INTO dbo.Orders (TableId, EmployeeId, OrderTime, Status)
        OUTPUT INSERTED.OrderId
        VALUES (@TableId, @EmployeeId, @OrderTime, @Status)";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TableId", order.Table!.TableId);
                cmd.Parameters.AddWithValue("@EmployeeId", order.Employee!.EmployeeId);
                cmd.Parameters.AddWithValue("@OrderTime", order.OrderTime);
                cmd.Parameters.AddWithValue("@Status", (int)order.Status);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding order: " + ex.Message);
            }
        }


        public void AddOrderItem(int orderId, OrderItem item)
        {
            const string query = @"
        INSERT INTO dbo.OrderItems (OrderId, MenuItemId, Quantity, Comment, Status, OrderTime)
        VALUES (@OrderId, @MenuItemId, @Quantity, @Comment, @Status, @OrderTime)";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@MenuItemId", item.MenuItem!.MenuItemId);
                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                cmd.Parameters.AddWithValue("@Comment", (object?)item.Comment ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (int)item.Status);
                cmd.Parameters.AddWithValue("@OrderTime", item.OrderTime);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding order item: " + ex.Message);
            }
        }

        public void DecreaseStock(int menuItemId, int quantity)
        {
            const string query = @"
        UPDATE dbo.MenuItems
        SET Stock = Stock - @Quantity
        WHERE MenuItemId = @MenuItemId
          AND Stock >= @Quantity";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);

            conn.Open();

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Not enough stock available for this menu item.");
            }
        }


        public void UpdateOrderItemQuantity(int orderItemId, int quantity)
        {
            const string query = "UPDATE dbo.OrderItems SET Quantity = @Quantity WHERE OrderItemId = @OrderItemId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating order item quantity: " + ex.Message);
            }
        }

        public void UpdateOrderItemComment(int orderItemId, string? comment)
        {
            const string query = "UPDATE dbo.OrderItems SET Comment = @Comment WHERE OrderItemId = @OrderItemId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
                cmd.Parameters.AddWithValue("@Comment", (object?)comment ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating order item comment: " + ex.Message);
            }
        }

        public void RemoveOrderItem(int orderItemId)
        {
            const string query = "DELETE FROM dbo.OrderItems WHERE OrderItemId = @OrderItemId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing order item: " + ex.Message);
            }
        }

        public void CancelOrder(int orderId)
        {
            // Use UPDATE instead of DELETE so order history is preserved
            // for Payment and Bar/Kitchen modules
            const string cancelItems = @"
                UPDATE dbo.OrderItems
                SET    Status = 5
                WHERE  OrderId = @OrderId";

            const string cancelOrder = @"
                UPDATE dbo.Orders
                SET    Status = 4
                WHERE  OrderId = @OrderId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                using var cmdItems = new SqlCommand(cancelItems, conn);
                cmdItems.Parameters.AddWithValue("@OrderId", orderId);
                cmdItems.ExecuteNonQuery();

                using var cmdOrder = new SqlCommand(cancelOrder, conn);
                cmdOrder.Parameters.AddWithValue("@OrderId", orderId);
                cmdOrder.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelling order: " + ex.Message);
            }
        }

        public List<OrderItem> GetItemsByOrderId(int orderId)
        {
            List<OrderItem> items = new List<OrderItem>();
            const string query = @"
        SELECT oi.OrderItemId, oi.Quantity, oi.Comment, oi.Status, oi.OrderTime,
               mi.MenuItemId, mi.Name, mi.Price, mi.Type, mi.Course, mi.Stock
        FROM dbo.OrderItems oi
        INNER JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
        WHERE oi.OrderId = @OrderId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new OrderItem
                    {
                        OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                        Status = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                        OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                        MenuItem = new MenuItem
                        {
                            MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Type = (MenuItemType)reader.GetInt32(reader.GetOrdinal("Type")),
                            Course = (CourseType)reader.GetInt32(reader.GetOrdinal("Course")),
                            Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching order items: " + ex.Message);
            }
            return items;
        }

        public void UpdateOrderStatus(int orderId, OrderStatus status)
        {
            const string query = "UPDATE dbo.Orders SET Status = @Status WHERE OrderId = @OrderId";
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@Status", (int)status);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating order status: " + ex.Message);
            }
        }


        public OrderItem? GetOrderItemById(int orderItemId)
        {
            const string query = @"
        SELECT oi.OrderItemId, oi.Quantity, oi.Comment, oi.Status, oi.OrderTime,
               mi.MenuItemId, mi.Name, mi.Price, mi.Type, mi.Course, mi.Stock
        FROM dbo.OrderItems oi
        INNER JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
        WHERE oi.OrderItemId = @OrderItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);

            conn.Open();

            using var reader = cmd.ExecuteReader();

            return reader.Read()
                ? MapOrderItemWithMenuItem(reader)
                : null;
        }

        public OrderItem? GetOrderItemByOrderAndMenuItem(int orderId, int menuItemId)
        {
            const string query = @"
        SELECT oi.OrderItemId, oi.Quantity, oi.Comment, oi.Status, oi.OrderTime,
               mi.MenuItemId, mi.Name, mi.Price, mi.Type, mi.Course, mi.Stock
        FROM dbo.OrderItems oi
        INNER JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
        WHERE oi.OrderId = @OrderId
          AND oi.MenuItemId = @MenuItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@OrderId", orderId);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);

            conn.Open();

            using var reader = cmd.ExecuteReader();

            return reader.Read()
                ? MapOrderItemWithMenuItem(reader)
                : null;
        }

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            const string query = @"
        SELECT MenuItemId, Name, Price, Type, Course, Stock
        FROM dbo.MenuItems
        WHERE MenuItemId = @MenuItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);

            conn.Open();

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new MenuItem
            {
                MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Type = (MenuItemType)reader.GetInt32(reader.GetOrdinal("Type")),
                Course = (CourseType)reader.GetInt32(reader.GetOrdinal("Course")),
                Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
            };
        }

        public void IncreaseStock(int menuItemId, int quantity)
        {
            const string query = @"
        UPDATE dbo.MenuItems
        SET Stock = Stock + @Quantity
        WHERE MenuItemId = @MenuItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);

            conn.Open();

            cmd.ExecuteNonQuery();
        }

        public void UpdateOrderItemStatus(int orderItemId, OrderItemStatus status)
        {
            const string query = "UPDATE dbo.OrderItems SET Status = @Status WHERE OrderItemId = @OrderItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
            cmd.Parameters.AddWithValue("@Status", (int)status);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool TableHasUnservedItems(int tableId)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM dbo.OrderItems oi
                INNER JOIN dbo.Orders o ON o.OrderId = oi.OrderId
                WHERE o.TableId = @TableId
                  AND o.Status  = 1
                  AND oi.Status NOT IN (4, 5)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@TableId", tableId);

            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        public List<OrderItem> GetSentItemsByTableId(int tableId)
        {
            var items = new List<OrderItem>();
            const string query = @"
                SELECT oi.OrderItemId, oi.Quantity, oi.Comment, oi.Status, oi.OrderTime,
                       mi.MenuItemId, mi.Name, mi.Price, mi.Type, mi.Course, mi.Stock
                FROM dbo.OrderItems oi
                INNER JOIN dbo.Orders o  ON o.OrderId    = oi.OrderId
                INNER JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
                WHERE o.TableId = @TableId
                  AND o.Status  = 2
                  AND CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                  AND oi.Status != 5
                ORDER BY oi.OrderTime";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TableId", tableId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    items.Add(MapOrderItemWithMenuItem(reader));
            }
            catch (SqlException) { }
            return items;
        }

        private static OrderItem MapOrderItemWithMenuItem(SqlDataReader reader)
        {
            return new OrderItem
            {
                OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                Comment = reader.IsDBNull(reader.GetOrdinal("Comment"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Comment")),
                Status = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                MenuItem = new MenuItem
                {
                    MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    Type = (MenuItemType)reader.GetInt32(reader.GetOrdinal("Type")),
                    Course = (CourseType)reader.GetInt32(reader.GetOrdinal("Course")),
                    Stock = reader.GetInt32(reader.GetOrdinal("Stock"))
                }
            };
        }



    }

}
