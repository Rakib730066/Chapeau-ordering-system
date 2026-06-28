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
                WHERE o.Status IN (@open, @submitted)
                AND   CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY o.OrderId, oi.OrderItemId";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@open",      (int)OrderStatus.Open);
                cmd.Parameters.AddWithValue("@submitted", (int)OrderStatus.Submitted);
                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int orderId = reader.GetInt32(reader.GetOrdinal("OrderId"));

                    if (!orders.TryGetValue(orderId, out Order? order))
                    {
                        order = MapOrderFromReader(reader);
                        orders[orderId] = order;
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("OrderItemId")))
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                            Quantity    = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Comment     = reader.IsDBNull(reader.GetOrdinal("Comment"))
                                              ? null
                                              : reader.GetString(reader.GetOrdinal("Comment")),
                            Status    = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("ItemStatus")),
                            OrderTime = reader.GetDateTime(reader.GetOrdinal("ItemOrderTime")),
                            StartedAt = reader.IsDBNull(reader.GetOrdinal("StartedAt"))
                                            ? default
                                            : reader.GetDateTime(reader.GetOrdinal("StartedAt")),
                            ReadyAt   = reader.IsDBNull(reader.GetOrdinal("ReadyAt"))
                                            ? default
                                            : reader.GetDateTime(reader.GetOrdinal("ReadyAt")),
                            MenuItem  = reader.IsDBNull(reader.GetOrdinal("MenuItemId"))
                                        ? null
                                        : new MenuItem
                                        {
                                            MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                                            Name       = reader.GetString(reader.GetOrdinal("MenuItemName")),
                                            Price      = reader.GetDecimal(reader.GetOrdinal("Price")),
                                            Type       = (MenuItemType)reader.GetInt32(reader.GetOrdinal("MenuItemType")),
                                            Course     = (CourseType)reader.GetInt32(reader.GetOrdinal("Course"))
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

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapOrderFromReader(reader) : null;
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
                AND   o.Status  = @open
                AND   CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY o.OrderTime DESC";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TableId", tableId);
            cmd.Parameters.AddWithValue("@open",    (int)OrderStatus.Open);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapOrderFromReader(reader) : null;
        }

        public int Add(Order order)
        {
            const string query = @"
        INSERT INTO dbo.Orders (TableId, EmployeeId, OrderTime, Status)
        OUTPUT INSERTED.OrderId
        VALUES (@TableId, @EmployeeId, @OrderTime, @Status)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TableId",    order.Table!.TableId);
            cmd.Parameters.AddWithValue("@EmployeeId", order.Employee!.EmployeeId);
            cmd.Parameters.AddWithValue("@OrderTime",  order.OrderTime);
            cmd.Parameters.AddWithValue("@Status",     (int)order.Status);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public void AddOrderItem(int orderId, OrderItem item)
        {
            const string query = @"
        INSERT INTO dbo.OrderItems (OrderId, MenuItemId, Quantity, Comment, Status, OrderTime)
        VALUES (@OrderId, @MenuItemId, @Quantity, @Comment, @Status, @OrderTime)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId",    orderId);
            cmd.Parameters.AddWithValue("@MenuItemId", item.MenuItem!.MenuItemId);
            cmd.Parameters.AddWithValue("@Quantity",   item.Quantity);
            cmd.Parameters.AddWithValue("@Comment",    (object?)item.Comment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status",     (int)item.Status);
            cmd.Parameters.AddWithValue("@OrderTime",  item.OrderTime);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void DecreaseStock(int menuItemId, int quantity)
        {
            const string query = @"
        UPDATE dbo.MenuItems
        SET Stock = Stock - @Quantity
        WHERE MenuItemId = @MenuItemId
          AND Stock >= @Quantity";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@Quantity",   quantity);
            conn.Open();

            if (cmd.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Not enough stock available for this menu item.");
        }

        public void UpdateOrderItemQuantity(int orderItemId, int quantity)
        {
            const string query = "UPDATE dbo.OrderItems SET Quantity = @Quantity WHERE OrderItemId = @OrderItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
            cmd.Parameters.AddWithValue("@Quantity",    quantity);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateOrderItemComment(int orderItemId, string? comment)
        {
            const string query = "UPDATE dbo.OrderItems SET Comment = @Comment WHERE OrderItemId = @OrderItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
            cmd.Parameters.AddWithValue("@Comment",     (object?)comment ?? DBNull.Value);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void RemoveOrderItem(int orderItemId)
        {
            const string query = "DELETE FROM dbo.OrderItems WHERE OrderItemId = @OrderItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void CancelOrder(int orderId)
        {
            // UPDATE instead of DELETE so order history is preserved for Payment and Bar/Kitchen modules
            const string cancelItems = @"
                UPDATE dbo.OrderItems
                SET    Status = @itemStatus
                WHERE  OrderId = @OrderId";

            const string cancelOrder = @"
                UPDATE dbo.Orders
                SET    Status = @orderStatus
                WHERE  OrderId = @OrderId";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmdItems = new SqlCommand(cancelItems, conn);
            cmdItems.Parameters.AddWithValue("@OrderId",    orderId);
            cmdItems.Parameters.AddWithValue("@itemStatus", (int)OrderItemStatus.Cancelled);
            cmdItems.ExecuteNonQuery();

            using var cmdOrder = new SqlCommand(cancelOrder, conn);
            cmdOrder.Parameters.AddWithValue("@OrderId",     orderId);
            cmdOrder.Parameters.AddWithValue("@orderStatus", (int)OrderStatus.Cancelled);
            cmdOrder.ExecuteNonQuery();
        }

        public List<OrderItem> GetItemsByOrderId(int orderId)
        {
            var items = new List<OrderItem>();
            const string query = @"
        SELECT oi.OrderItemId, oi.Quantity, oi.Comment, oi.Status, oi.OrderTime,
               mi.MenuItemId, mi.Name, mi.Price, mi.Type, mi.Course, mi.Stock
        FROM dbo.OrderItems oi
        INNER JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
        WHERE oi.OrderId = @OrderId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                items.Add(MapOrderItemWithMenuItem(reader));
            return items;
        }

        public void UpdateOrderStatus(int orderId, OrderStatus status)
        {
            const string query = "UPDATE dbo.Orders SET Status = @Status WHERE OrderId = @OrderId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            cmd.Parameters.AddWithValue("@Status",  (int)status);
            conn.Open();
            cmd.ExecuteNonQuery();
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
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapOrderItemWithMenuItem(reader) : null;
        }

        public OrderItem? GetOrderItemByOrderAndMenuItem(int orderId, int menuItemId)
        {
            const string query = @"
        SELECT oi.OrderItemId, oi.Quantity, oi.Comment, oi.Status, oi.OrderTime,
               mi.MenuItemId, mi.Name, mi.Price, mi.Type, mi.Course, mi.Stock
        FROM dbo.OrderItems oi
        INNER JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
        WHERE oi.OrderId    = @OrderId
          AND oi.MenuItemId = @MenuItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderId",    orderId);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapOrderItemWithMenuItem(reader) : null;
        }

        public MenuItem? GetMenuItemById(int menuItemId)
        {
            const string query = @"
        SELECT MenuItemId, Name, Price, Type, Course, Stock
        FROM dbo.MenuItems
        WHERE MenuItemId = @MenuItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new MenuItem
            {
                MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                Name       = reader.GetString(reader.GetOrdinal("Name")),
                Price      = reader.GetDecimal(reader.GetOrdinal("Price")),
                Type       = (MenuItemType)reader.GetInt32(reader.GetOrdinal("Type")),
                Course     = (CourseType)reader.GetInt32(reader.GetOrdinal("Course")),
                Stock      = reader.GetInt32(reader.GetOrdinal("Stock"))
            };
        }

        public void IncreaseStock(int menuItemId, int quantity)
        {
            const string query = @"
        UPDATE dbo.MenuItems
        SET Stock = Stock + @Quantity
        WHERE MenuItemId = @MenuItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MenuItemId", menuItemId);
            cmd.Parameters.AddWithValue("@Quantity",   quantity);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateOrderItemStatus(int orderItemId, OrderItemStatus status)
        {
            const string query = "UPDATE dbo.OrderItems SET Status = @Status WHERE OrderItemId = @OrderItemId";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OrderItemId", orderItemId);
            cmd.Parameters.AddWithValue("@Status",      (int)status);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool TableHasUnservedItems(int tableId)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM dbo.OrderItems oi
                INNER JOIN dbo.Orders o ON o.OrderId = oi.OrderId
                WHERE o.TableId   = @TableId
                  AND o.Status    = @open
                  AND oi.Status NOT IN (@served, @cancelled)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TableId",   tableId);
            cmd.Parameters.AddWithValue("@open",      (int)OrderStatus.Open);
            cmd.Parameters.AddWithValue("@served",    (int)OrderItemStatus.Served);
            cmd.Parameters.AddWithValue("@cancelled", (int)OrderItemStatus.Cancelled);
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
                INNER JOIN dbo.Orders   o  ON o.OrderId     = oi.OrderId
                INNER JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
                WHERE o.TableId  = @TableId
                  AND o.Status   = @submitted
                  AND CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                  AND oi.Status != @cancelled
                ORDER BY oi.OrderTime";

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TableId",   tableId);
                cmd.Parameters.AddWithValue("@submitted", (int)OrderStatus.Submitted);
                cmd.Parameters.AddWithValue("@cancelled", (int)OrderItemStatus.Cancelled);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    items.Add(MapOrderItemWithMenuItem(reader));
            }
            catch (SqlException)
            {
                return items;
            }
            return items;
        }

        // ── Private mapping helpers ───────────────────────────────────────────────

        private static Order MapOrderFromReader(SqlDataReader reader) => new Order
        {
            OrderId   = reader.GetInt32(reader.GetOrdinal("OrderId")),
            OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
            Status    = (OrderStatus)reader.GetInt32(reader.GetOrdinal("OrderStatus")),
            Table = new RestaurantTable
            {
                TableId       = reader.GetInt32(reader.GetOrdinal("TableId")),
                TableNumber   = reader.GetString(reader.GetOrdinal("TableNumber")),
                NumberOfSeats = reader.GetInt32(reader.GetOrdinal("NumberOfSeats")),
                Status        = (TableStatus)reader.GetInt32(reader.GetOrdinal("TableStatus")),
                OccupiedSince = reader.IsDBNull(reader.GetOrdinal("OccupiedSince"))
                                ? null
                                : reader.GetDateTime(reader.GetOrdinal("OccupiedSince"))
            },
            Employee = new Employee
            {
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                FirstName  = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName   = reader.GetString(reader.GetOrdinal("LastName"))
            }
        };

        private static OrderItem MapOrderItemWithMenuItem(SqlDataReader reader) => new OrderItem
        {
            OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
            Quantity    = reader.GetInt32(reader.GetOrdinal("Quantity")),
            Comment     = reader.IsDBNull(reader.GetOrdinal("Comment"))
                          ? null
                          : reader.GetString(reader.GetOrdinal("Comment")),
            Status    = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("Status")),
            OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
            MenuItem  = new MenuItem
            {
                MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                Name       = reader.GetString(reader.GetOrdinal("Name")),
                Price      = reader.GetDecimal(reader.GetOrdinal("Price")),
                Type       = (MenuItemType)reader.GetInt32(reader.GetOrdinal("Type")),
                Course     = (CourseType)reader.GetInt32(reader.GetOrdinal("Course")),
                Stock      = reader.GetInt32(reader.GetOrdinal("Stock"))
            }
        };
    }
}
