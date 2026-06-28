using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

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
            conn.Open();
            using SqlDataReader reader = ExecuteOrderQuery(conn, tableId);
            return BuildOrderFromReader(reader);
        }

        public void FinishOrder(Payment payment)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            using SqlTransaction transaction = conn.BeginTransaction();
            try
            {
                InsertPayment(conn, transaction, payment);
                MarkOrderAsPaid(conn, transaction, payment.OrderId);
                FreeTable(conn, transaction, payment.OrderId);
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
                InsertMultiplePayments(conn, transaction, payments);
                MarkOrderAsPaid(conn, transaction, orderId);
                FreeTable(conn, transaction, orderId);
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
            var vm = new FinancialOverviewViewModel { StartDate = startDate, EndDate = endDate };
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            PopulateDailyBreakdown(conn, vm, startDate, endDate);
            PopulateCardRevenue(conn, vm, startDate, endDate);
            return vm;
        }

        // ===== PRIVATE HELPERS (all <= 10 lines) =====

        private SqlDataReader ExecuteOrderQuery(SqlConnection conn, int tableId)
        {
            string query = @"
                SELECT  o.OrderId, o.TableId, t.TableNumber, o.OrderTime, o.Status,
                        oi.OrderItemId, oi.Quantity, oi.Comment, oi.Status AS ItemStatus,
                        mi.MenuItemId, mi.Name, mi.Price, mi.Type, mi.Course, mi.VatRate
                FROM dbo.Orders o
                JOIN dbo.OrderItems oi ON oi.OrderId = o.OrderId
                JOIN dbo.Tables t ON t.TableId = o.TableId
                JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
                WHERE o.TableId = @TableId AND o.Status IN (@OpenStatus, @SubmittedStatus)
                ORDER BY o.OrderId, mi.Name";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@TableId", SqlDbType.Int).Value = tableId;
            cmd.Parameters.Add("@OpenStatus", SqlDbType.Int).Value = (int)OrderStatus.Open;
            cmd.Parameters.Add("@SubmittedStatus", SqlDbType.Int).Value = (int)OrderStatus.Submitted;
            return cmd.ExecuteReader();
        }

        private Order? BuildOrderFromReader(SqlDataReader reader)
        {
            Order? order = null;
            while (reader.Read())
            {
                order ??= CreateOrderHeader(reader);
                order.OrderItems.Add(CreateOrderItem(reader));
            }
            return order;
        }

        private Order CreateOrderHeader(SqlDataReader reader)
        {
            int tableNumberOrd = reader.GetOrdinal("TableNumber");
            return new Order
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                Status = (OrderStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                Table = new RestaurantTable
                {
                    TableId = reader.GetInt32(reader.GetOrdinal("TableId")),
                    TableNumber = reader.IsDBNull(tableNumberOrd) ? string.Empty : reader.GetString(tableNumberOrd)
                },
                OrderItems = new List<OrderItem>()
            };
        }

        private OrderItem CreateOrderItem(SqlDataReader reader)
        {
            int commentOrd = reader.GetOrdinal("Comment");
            return new OrderItem
            {
                OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                Comment = reader.IsDBNull(commentOrd) ? null : reader.GetString(commentOrd),
                Status = (OrderItemStatus)reader.GetInt32(reader.GetOrdinal("ItemStatus")),
                MenuItem = CreateMenuItem(reader)
            };
        }

        private MenuItem CreateMenuItem(SqlDataReader reader)
        {
            return new MenuItem
            {
                MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Type = (MenuItemType)reader.GetInt32(reader.GetOrdinal("Type")),
                Course = (CourseType)reader.GetInt32(reader.GetOrdinal("Course")),
                VatRate = reader.GetDecimal(reader.GetOrdinal("VatRate"))
            };
        }

        private void InsertPayment(SqlConnection conn, SqlTransaction transaction, Payment payment)
        {
            string query = @"
                INSERT INTO dbo.Payments (OrderId, Amount, TipAmount, VatLowAmount, VatHighAmount, PaymentMethod, Feedback, PaidAt)
                VALUES (@OrderId, @Amount, @TipAmount, @VatLow, @VatHigh, @Method, @Feedback, @PaidAt)";

            using SqlCommand cmd = new SqlCommand(query, conn, transaction);
            PopulatePaymentParameters(cmd, payment);
            cmd.ExecuteNonQuery();
        }

        private void InsertMultiplePayments(SqlConnection conn, SqlTransaction transaction, List<Payment> payments)
        {
            foreach (Payment payment in payments)
                InsertPayment(conn, transaction, payment);
        }

        private void PopulatePaymentParameters(SqlCommand cmd, Payment payment)
        {
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = payment.OrderId;
            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = payment.Amount;
            cmd.Parameters.Add("@TipAmount", SqlDbType.Decimal).Value = payment.TipAmount;
            cmd.Parameters.Add("@VatLow", SqlDbType.Decimal).Value = payment.VatLowAmount;
            cmd.Parameters.Add("@VatHigh", SqlDbType.Decimal).Value = payment.VatHighAmount;
            cmd.Parameters.Add("@Method", SqlDbType.Int).Value = (int)payment.PaymentMethod;
            cmd.Parameters.Add("@Feedback", SqlDbType.NVarChar).Value = (object?)payment.Feedback ?? DBNull.Value;
            cmd.Parameters.Add("@PaidAt", SqlDbType.DateTime).Value = payment.PaidAt;
        }

        private void MarkOrderAsPaid(SqlConnection conn, SqlTransaction transaction, int orderId)
        {
            using SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.Orders SET Status = @Paid WHERE OrderId = @OrderId", conn, transaction);
            cmd.Parameters.Add("@Paid", SqlDbType.Int).Value = (int)OrderStatus.Paid;
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.ExecuteNonQuery();
        }

        private void FreeTable(SqlConnection conn, SqlTransaction transaction, int orderId)
        {
            using SqlCommand cmd = new SqlCommand(@"
                UPDATE dbo.Tables SET Status = 0, CurrentOrderId = NULL, OccupiedSince = NULL, LastUpdated = SYSUTCDATETIME()
                WHERE TableId = (SELECT TableId FROM dbo.Orders WHERE OrderId = @OrderId)", conn, transaction);
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.ExecuteNonQuery();
        }

        private void PopulateDailyBreakdown(SqlConnection conn, FinancialOverviewViewModel vm, DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT CAST(p.PaidAt AS DATE) AS PayDate, SUM(p.Amount) AS TotalRevenue, SUM(p.TipAmount) AS TotalTip,
                       SUM(p.VatLowAmount) AS TotalVatLow, SUM(p.VatHighAmount) AS TotalVatHigh, COUNT(DISTINCT p.OrderId) AS TotalOrders
                FROM dbo.Payments p WHERE p.PaidAt >= @Start AND p.PaidAt < @End GROUP BY CAST(p.PaidAt AS DATE) ORDER BY PayDate";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@Start", SqlDbType.DateTime).Value = startDate.Date;
            cmd.Parameters.Add("@End", SqlDbType.DateTime).Value = endDate.Date.AddDays(1);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                AccumulateDaily(vm, reader);
        }

        private void AccumulateDaily(FinancialOverviewViewModel vm, SqlDataReader reader)
        {
            decimal rev = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"));
            decimal tip = reader.GetDecimal(reader.GetOrdinal("TotalTip"));
            vm.TotalRevenue += rev;
            vm.TotalTip += tip;
            vm.TotalVatLow += reader.GetDecimal(reader.GetOrdinal("TotalVatLow"));
            vm.TotalVatHigh += reader.GetDecimal(reader.GetOrdinal("TotalVatHigh"));
            vm.TotalOrders += reader.GetInt32(reader.GetOrdinal("TotalOrders"));
            vm.DailyBreakdown.Add(new DailyRevenueLine
            {
                Date = reader.GetDateTime(reader.GetOrdinal("PayDate")),
                Revenue = rev,
                Tip = tip,
                Orders = reader.GetInt32(reader.GetOrdinal("TotalOrders"))
            });
        }

        private void PopulateCardRevenue(SqlConnection conn, FinancialOverviewViewModel vm, DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT mi.Card, SUM(oi.Quantity * mi.Price) AS Revenue
                FROM dbo.Payments p JOIN dbo.Orders o ON o.OrderId = p.OrderId
                JOIN dbo.OrderItems oi ON oi.OrderId = o.OrderId JOIN dbo.MenuItems mi ON mi.MenuItemId = oi.MenuItemId
                WHERE p.PaidAt >= @Start AND p.PaidAt < @End GROUP BY mi.Card";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@Start", SqlDbType.DateTime).Value = startDate.Date;
            cmd.Parameters.Add("@End", SqlDbType.DateTime).Value = endDate.Date.AddDays(1);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                AssignCardRevenue(vm, reader.GetInt32(0), reader.GetDecimal(1));
        }

        private void AssignCardRevenue(FinancialOverviewViewModel vm, int card, decimal revenue)
        {
            if (card == (int)CardType.Lunch) vm.LunchRevenue = revenue;
            else if (card == (int)CardType.Dinner) vm.DinnerRevenue = revenue;
            else if (card == (int)CardType.Drinks) vm.DrinksRevenue = revenue;
        }
    }
}