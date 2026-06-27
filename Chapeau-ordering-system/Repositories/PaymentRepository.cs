using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.ViewModels;
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
                AND o.Status IN (@OpenStatus, @SubmittedStatus)
                ORDER BY o.OrderId, mi.Name";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@TableId", System.Data.SqlDbType.Int).Value = tableId;
            cmd.Parameters.Add("@OpenStatus", System.Data.SqlDbType.Int).Value = (int)OrderStatus.Open;
            cmd.Parameters.Add("@SubmittedStatus", System.Data.SqlDbType.Int).Value = (int)OrderStatus.Submitted;

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

        public void FinishOrder(Payment payment)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                string insertPayment = @"
                    INSERT INTO dbo.Payments
                        (OrderId, Amount, TipAmount, VatLowAmount, VatHighAmount,
                         PaymentMethod, Feedback, PaidAt)
                    VALUES
                        (@OrderId, @Amount, @TipAmount, @VatLow, @VatHigh,
                         @Method, @Feedback, @PaidAt)";

                SqlCommand cmd1 = new SqlCommand(insertPayment, conn, transaction);
                cmd1.Parameters.Add("@OrderId", System.Data.SqlDbType.Int).Value = payment.OrderId;
                cmd1.Parameters.Add("@Amount", System.Data.SqlDbType.Decimal).Value = payment.Amount;
                cmd1.Parameters.Add("@TipAmount", System.Data.SqlDbType.Decimal).Value = payment.TipAmount;
                cmd1.Parameters.Add("@VatLow", System.Data.SqlDbType.Decimal).Value = payment.VatLowAmount;
                cmd1.Parameters.Add("@VatHigh", System.Data.SqlDbType.Decimal).Value = payment.VatHighAmount;
                cmd1.Parameters.Add("@Method", System.Data.SqlDbType.Int).Value = (int)payment.PaymentMethod;
                cmd1.Parameters.Add("@Feedback", System.Data.SqlDbType.NVarChar).Value =
                    (object?)payment.Feedback ?? DBNull.Value;
                cmd1.Parameters.Add("@PaidAt", System.Data.SqlDbType.DateTime).Value = payment.PaidAt;
                cmd1.ExecuteNonQuery();

                SqlCommand cmd2 = new SqlCommand(
                    "UPDATE dbo.Orders SET Status = @PaidStatus WHERE OrderId = @OrderId",
                    conn, transaction);
                cmd2.Parameters.Add("@PaidStatus", System.Data.SqlDbType.Int).Value = (int)OrderStatus.Paid;
                cmd2.Parameters.Add("@OrderId", System.Data.SqlDbType.Int).Value = payment.OrderId;
                cmd2.ExecuteNonQuery();

                SqlCommand cmd3 = new SqlCommand(@"
                    UPDATE dbo.Tables
                    SET Status = 0,
                        CurrentOrderId = NULL,
                        OccupiedSince = NULL,
                        LastUpdated = SYSUTCDATETIME()
                    WHERE TableId = (SELECT TableId FROM dbo.Orders WHERE OrderId = @OrderId)",
                    conn, transaction);
                cmd3.Parameters.Add("@OrderId", System.Data.SqlDbType.Int).Value = payment.OrderId;
                cmd3.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void FinishOrderWithSplit(List<Payment> payments)
        {
            if (payments == null || payments.Count == 0)
                throw new ArgumentException("At least one payment is required.");

            int orderId = payments[0].OrderId;

            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                foreach (Payment payment in payments)
                {
                    string insertPayment = @"
                        INSERT INTO dbo.Payments
                            (OrderId, Amount, TipAmount, VatLowAmount, VatHighAmount,
                             PaymentMethod, Feedback, PaidAt)
                        VALUES
                            (@OrderId, @Amount, @TipAmount, @VatLow, @VatHigh,
                             @Method, @Feedback, @PaidAt)";

                    SqlCommand cmd = new SqlCommand(insertPayment, conn, transaction);
                    cmd.Parameters.Add("@OrderId", System.Data.SqlDbType.Int).Value = payment.OrderId;
                    cmd.Parameters.Add("@Amount", System.Data.SqlDbType.Decimal).Value = payment.Amount;
                    cmd.Parameters.Add("@TipAmount", System.Data.SqlDbType.Decimal).Value = payment.TipAmount;
                    cmd.Parameters.Add("@VatLow", System.Data.SqlDbType.Decimal).Value = payment.VatLowAmount;
                    cmd.Parameters.Add("@VatHigh", System.Data.SqlDbType.Decimal).Value = payment.VatHighAmount;
                    cmd.Parameters.Add("@Method", System.Data.SqlDbType.Int).Value = (int)payment.PaymentMethod;
                    cmd.Parameters.Add("@Feedback", System.Data.SqlDbType.NVarChar).Value =
                        (object?)payment.Feedback ?? DBNull.Value;
                    cmd.Parameters.Add("@PaidAt", System.Data.SqlDbType.DateTime).Value = payment.PaidAt;
                    cmd.ExecuteNonQuery();
                }

                SqlCommand markPaid = new SqlCommand(
                    "UPDATE dbo.Orders SET Status = @Paid WHERE OrderId = @OrderId",
                    conn, transaction);
                markPaid.Parameters.Add("@Paid", System.Data.SqlDbType.Int).Value = (int)OrderStatus.Paid;
                markPaid.Parameters.Add("@OrderId", System.Data.SqlDbType.Int).Value = orderId;
                markPaid.ExecuteNonQuery();

                SqlCommand freeTable = new SqlCommand(@"
                    UPDATE dbo.Tables
                    SET Status = 0,
                        CurrentOrderId = NULL,
                        OccupiedSince = NULL,
                        LastUpdated = SYSUTCDATETIME()
                    WHERE TableId = (SELECT TableId FROM dbo.Orders WHERE OrderId = @OrderId)",
                    conn, transaction);
                freeTable.Parameters.Add("@OrderId", System.Data.SqlDbType.Int).Value = orderId;
                freeTable.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public FinancialOverviewViewModel GetFinancialOverview(DateTime startDate, DateTime endDate)
        {
            var vm = new FinancialOverviewViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            string query = @"
                SELECT
                    CAST(p.PaidAt AS DATE)         AS PayDate,
                    SUM(p.Amount)                  AS TotalRevenue,
                    SUM(p.TipAmount)               AS TotalTip,
                    SUM(p.VatLowAmount)            AS TotalVatLow,
                    SUM(p.VatHighAmount)           AS TotalVatHigh,
                    COUNT(DISTINCT p.OrderId)      AS TotalOrders
                FROM dbo.Payments p
                WHERE p.PaidAt >= @Start AND p.PaidAt < @End
                GROUP BY CAST(p.PaidAt AS DATE)
                ORDER BY PayDate";

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@Start", System.Data.SqlDbType.DateTime).Value = startDate.Date;
            cmd.Parameters.Add("@End",   System.Data.SqlDbType.DateTime).Value = endDate.Date.AddDays(1);
            conn.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                decimal rev = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"));
                decimal tip = reader.GetDecimal(reader.GetOrdinal("TotalTip"));
                vm.TotalRevenue += rev;
                vm.TotalTip     += tip;
                vm.TotalVatLow  += reader.GetDecimal(reader.GetOrdinal("TotalVatLow"));
                vm.TotalVatHigh += reader.GetDecimal(reader.GetOrdinal("TotalVatHigh"));
                vm.TotalOrders  += reader.GetInt32(reader.GetOrdinal("TotalOrders"));
                vm.DailyBreakdown.Add(new DailyRevenueLine
                {
                    Date    = reader.GetDateTime(reader.GetOrdinal("PayDate")),
                    Revenue = rev,
                    Tip     = tip,
                    Orders  = reader.GetInt32(reader.GetOrdinal("TotalOrders"))
                });
            }
            reader.Close();

            string cardQuery = @"
                SELECT mi.Card, SUM(oi.Quantity * mi.Price) AS Revenue
                FROM dbo.Payments p
                JOIN dbo.Orders o      ON o.OrderId = p.OrderId
                JOIN dbo.OrderItems oi ON oi.OrderId = o.OrderId
                JOIN dbo.MenuItems mi  ON mi.MenuItemId = oi.MenuItemId
                WHERE p.PaidAt >= @Start AND p.PaidAt < @End
                GROUP BY mi.Card";

            using SqlCommand cmd2 = new SqlCommand(cardQuery, conn);
            cmd2.Parameters.Add("@Start", System.Data.SqlDbType.DateTime).Value = startDate.Date;
            cmd2.Parameters.Add("@End",   System.Data.SqlDbType.DateTime).Value = endDate.Date.AddDays(1);
            using SqlDataReader r2 = cmd2.ExecuteReader();
            while (r2.Read())
            {
                int card = r2.GetInt32(0);
                decimal rev = r2.GetDecimal(1);
                if (card == (int)CardType.Lunch)  vm.LunchRevenue  = rev;
                if (card == (int)CardType.Dinner) vm.DinnerRevenue = rev;
                if (card == (int)CardType.Drinks) vm.DrinksRevenue = rev;
            }

            return vm;
        }
    }
}
