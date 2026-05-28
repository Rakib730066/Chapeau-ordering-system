using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Chapeau_ordering_system.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDatabase")!;
        }

        public Order? GetOpenOrderByTable(int tableId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string query = @"
                SELECT  o.OrderId,
                        o.TableId,
                        t.TableNumber,
                        o.OrderTime,
                        o.Status,
                        oi.OrderItemId,
                        oi.Quantity,
                        oi.Comment,
                        oi.Status      AS ItemStatus,
                        mi.MenuItemId,
                        mi.Name,
                        mi.Price,
                        mi.Type,
                        mi.Course,
                        mi.VatRate
                FROM dbo.Orders o
                JOIN dbo.OrderItems oi ON oi.OrderId = o.OrderId
                JOIN dbo.Tables t      ON t.TableId = o.TableId
                JOIN dbo.MenuItems mi  ON mi.MenuItemId = oi.MenuItemId
                WHERE o.TableId = @TableId
                  AND o.Status = @OpenStatus
                ORDER BY o.OrderId, mi.Name";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@TableId", System.Data.SqlDbType.Int).Value = tableId;
            cmd.Parameters.Add("@OpenStatus", System.Data.SqlDbType.Int).Value = (int)OrderStatus.Open;

            conn.Open();
            using SqlDataReader reader = cmd.ExecuteReader();

            Order? order = null;

            while (reader.Read())
            {
                if (order == null)
                {
                    order = new Order
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                        Status = (OrderStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                        Table = new RestaurantTable(),
                        OrderItems = new List<OrderItem>()
                    };

                    order.Table.TableId = reader.GetInt32(reader.GetOrdinal("TableId"));

                    int tableNumberOrd = reader.GetOrdinal("TableNumber");
                    order.Table.TableNumber = reader.IsDBNull(tableNumberOrd)
                        ? string.Empty
                        : reader.GetString(tableNumberOrd);
                }

                MenuItem menuItem = new MenuItem
                {
                    MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    Type = (MenuItemType)reader.GetInt32(reader.GetOrdinal("Type")),
                    Course = (CourseType)reader.GetInt32(reader.GetOrdinal("Course")),
                    VatRate = reader.GetDecimal(reader.GetOrdinal("VatRate"))
                };

                int commentOrd = reader.GetOrdinal("Comment");

                OrderItem orderItem = new OrderItem
                {
                    OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    Comment = reader.IsDBNull(commentOrd) ? null : reader.GetString(commentOrd),
                    Status = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("ItemStatus")),
                    MenuItem = menuItem
                };

                order.OrderItems.Add(orderItem);
            }

            return order;
        }
    }
}