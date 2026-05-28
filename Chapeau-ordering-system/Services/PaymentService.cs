using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public OrderPaymentViewModel? GetOrderForPayment(int tableId)
        {
            Order? order = _paymentRepository.GetOpenOrderByTable(tableId);
            if (order == null) return null;
            return BuildViewModel(order);
        }

        public FinishOrderViewModel? GetFinishOrderViewModel(int tableId)
        {
            Order? order = _paymentRepository.GetOpenOrderByTable(tableId);
            if (order == null) return null;

            OrderPaymentViewModel summary = BuildViewModel(order);

            return new FinishOrderViewModel
            {
                OrderId = summary.OrderId,
                TableId = summary.TableId,
                TableNumber = summary.TableNumber,
                TotalToPay = summary.TotalInclVat,
                VatLow = summary.VatLow,
                VatHigh = summary.VatHigh,
                AmountPaid = summary.TotalInclVat  // pre-fill with the bill total
            };
        }

        public void FinishOrder(FinishOrderViewModel input)
        {
            // Re-fetch the order to recompute VAT server-side
            // (never trust amounts coming from the browser)
            Order? order = _paymentRepository.GetOpenOrderByTable(input.TableId);
            if (order == null)
                throw new InvalidOperationException("No open order for this table.");

            OrderPaymentViewModel summary = BuildViewModel(order);

            decimal tip = input.AmountPaid - summary.TotalInclVat;
            if (tip < 0m) tip = 0m;  // never allow negative tip

            Payment payment = new Payment
            {
                OrderId = summary.OrderId,
                Amount = summary.TotalInclVat,        // bill amount (excl. tip)
                TipAmount = Math.Round(tip, 2),
                VatLowAmount = summary.VatLow,
                VatHighAmount = summary.VatHigh,
                PaymentMethod = input.PaymentMethod,
                Feedback = string.IsNullOrWhiteSpace(input.Feedback) ? null : input.Feedback.Trim(),
                PaidAt = DateTime.Now
            };

            _paymentRepository.FinishOrder(payment);
        }

        // --- private helper ---
        private OrderPaymentViewModel BuildViewModel(Order order)
        {
            var vm = new OrderPaymentViewModel
            {
                OrderId = order.OrderId,
                TableId = order.Table!.TableId,
                TableNumber = order.Table.TableNumber
            };

            var grouped = order.OrderItems
                .Where(oi => oi.Status != OrderItemStatus.Cancelled)
                .GroupBy(oi => oi.MenuItem!.MenuItemId)
                .Select(g => new OrderLineViewModel
                {
                    MenuItemName = g.First().MenuItem!.Name,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.First().MenuItem!.Price,
                    VatRate = g.First().MenuItem!.VatRate
                })
                .ToList();

            vm.Lines = grouped;

            decimal vatLow = 0m, vatHigh = 0m, total = 0m;

            foreach (var line in grouped)
            {
                total += line.LineTotal;
                decimal vatPortion = line.LineTotal * (line.VatRate / (100m + line.VatRate));

                if (line.VatRate == 9m) vatLow += vatPortion;
                else if (line.VatRate == 21m) vatHigh += vatPortion;
            }

            vm.TotalInclVat = Math.Round(total, 2);
            vm.VatLow = Math.Round(vatLow, 2);
            vm.VatHigh = Math.Round(vatHigh, 2);

            return vm;
        }
    }
}