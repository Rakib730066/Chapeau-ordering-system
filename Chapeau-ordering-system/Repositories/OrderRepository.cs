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
                WHERE o.Status = 1
                ORDER BY o.OrderId, oi.OrderItemId";

            try
            {
            using var conn = new SqlConnection(_connectionString);
            using var cmd  = new SqlCommand(query, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int orderId = reader.GetInt32(reader.GetOrdinal("OrderId"));

                if (!orders.TryGetValue(orderId, out Order? order))
                {
                    order = new Order
                    {
                        OrderId   = orderId,
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
                        Status      = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("ItemStatus")),
                        OrderTime   = reader.GetDateTime(reader.GetOrdinal("ItemOrderTime")),
                        StartedAt   = reader.IsDBNull(reader.GetOrdinal("StartedAt"))
                                      ? default
                                      : reader.GetDateTime(reader.GetOrdinal("StartedAt")),
                        ReadyAt     = reader.IsDBNull(reader.GetOrdinal("ReadyAt"))
                                      ? default
                                      : reader.GetDateTime(reader.GetOrdinal("ReadyAt")),
                        MenuItem = reader.IsDBNull(reader.GetOrdinal("MenuItemId"))
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
    }
}
