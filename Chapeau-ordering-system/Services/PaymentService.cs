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
                AmountPaid = summary.TotalInclVat
            };
        }

        public void FinishOrder(FinishOrderViewModel input)
        {
            Order? order = _paymentRepository.GetOpenOrderByTable(input.TableId);
            if (order == null)
                throw new InvalidOperationException("No open order for this table.");

            OrderPaymentViewModel summary = BuildViewModel(order);

            decimal tip = input.AmountPaid - summary.TotalInclVat;
            if (tip < 0m) tip = 0m;

            Payment payment = new Payment
            {
                OrderId = summary.OrderId,
                Amount = summary.TotalInclVat,
                TipAmount = Math.Round(tip, 2),
                VatLowAmount = summary.VatLow,
                VatHighAmount = summary.VatHigh,
                PaymentMethod = input.PaymentMethod,
                Feedback = string.IsNullOrWhiteSpace(input.Feedback) ? null : input.Feedback.Trim(),
                PaidAt = DateTime.Now
            };

            _paymentRepository.FinishOrder(payment);
        }

        public SplitPaymentViewModel? GetSplitPaymentViewModel(int tableId)
        {
            Order? order = _paymentRepository.GetOpenOrderByTable(tableId);
            if (order == null) return null;

            OrderPaymentViewModel summary = BuildViewModel(order);

            // Pre-fill 2 rows for default equal split (2 people)
            var viewModel = new SplitPaymentViewModel
            {
                OrderId = summary.OrderId,
                TableId = summary.TableId,
                TableNumber = summary.TableNumber,
                TotalToPay = summary.TotalInclVat,
                VatLow = summary.VatLow,
                VatHigh = summary.VatHigh,
                Mode = SplitMode.Equal,
                NumberOfPeople = 2
            };

            decimal share = Math.Round(summary.TotalInclVat / 2m, 2);
            for (int i = 0; i < 2; i++)
            {
                viewModel.Payments.Add(new PersonPaymentViewModel
                {
                    AmountPaid = share,
                    PaymentMethod = PaymentMethod.Cash
                });
            }

            return viewModel;
        }

        public (bool success, string? errorMessage) FinishSplitOrder(SplitPaymentViewModel input)
        {
            Order? order = _paymentRepository.GetOpenOrderByTable(input.TableId);
            if (order == null)
                return (false, "No open order for this table.");

            OrderPaymentViewModel summary = BuildViewModel(order);

            // Validate: must have at least 1 payment row
            if (input.Payments == null || input.Payments.Count == 0)
                return (false, "At least one payment is required.");

            // Validate: sum of all payments must be at least the bill total
            decimal sumPaid = input.Payments.Sum(p => p.AmountPaid);
            if (sumPaid < summary.TotalInclVat)
            {
                decimal remaining = summary.TotalInclVat - sumPaid;
                return (false, $"Payments only cover €{sumPaid:0.00} of €{summary.TotalInclVat:0.00}. " +
                               $"Remaining to pay: €{remaining:0.00}");
            }

            // Build per-person Payment rows.
            // For VAT allocation, each person gets a proportional share of the bill's VAT
            // (based on what fraction of the bill they paid).
            List<Payment> payments = new List<Payment>();

            foreach (var person in input.Payments)
            {
                // What fraction of the bill total did this person cover?
                decimal billShare = person.AmountPaid > summary.TotalInclVat
                    ? summary.TotalInclVat
                    : person.AmountPaid;

                decimal shareFraction = summary.TotalInclVat == 0
                    ? 0
                    : billShare / summary.TotalInclVat;

                decimal personVatLow = Math.Round(summary.VatLow * shareFraction, 2);
                decimal personVatHigh = Math.Round(summary.VatHigh * shareFraction, 2);
                decimal personTip = person.AmountPaid - billShare;
                if (personTip < 0) personTip = 0;

                payments.Add(new Payment
                {
                    OrderId = summary.OrderId,
                    Amount = Math.Round(billShare, 2),
                    TipAmount = Math.Round(personTip, 2),
                    VatLowAmount = personVatLow,
                    VatHighAmount = personVatHigh,
                    PaymentMethod = person.PaymentMethod,
                    Feedback = string.IsNullOrWhiteSpace(person.Feedback) ? null : person.Feedback.Trim(),
                    PaidAt = DateTime.Now
                });
            }

            _paymentRepository.FinishOrderWithSplit(payments);
            return (true, null);
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