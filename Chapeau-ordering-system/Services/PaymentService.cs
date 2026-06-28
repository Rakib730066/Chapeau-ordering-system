using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Services
{
    public class PaymentService : IPaymentService
    {
        private const int DefaultNumberOfPeople = 2;

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

            decimal tip = CalculateTip(input, summary.TotalInclVat);

            Payment payment = new Payment
            {
                OrderId = summary.OrderId,
                Amount = summary.TotalInclVat,
                TipAmount = tip,
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

            var viewModel = new SplitPaymentViewModel
            {
                OrderId = summary.OrderId,
                TableId = summary.TableId,
                TableNumber = summary.TableNumber,
                TotalToPay = summary.TotalInclVat,
                VatLow = summary.VatLow,
                VatHigh = summary.VatHigh,
                Mode = SplitMode.Equal,
                NumberOfPeople = DefaultNumberOfPeople
            };

            viewModel.Payments = BuildEqualSplitRows(viewModel.TotalToPay, DefaultNumberOfPeople);
            return viewModel;
        }

        public SplitPaymentViewModel RebuildEqualSplit(SplitPaymentViewModel input)
        {
            if (input.Mode == SplitMode.Equal && input.NumberOfPeople >= DefaultNumberOfPeople)
            {
                input.Payments = BuildEqualSplitRows(input.TotalToPay, input.NumberOfPeople);
            }

            return input;
        }

        public (bool success, string? errorMessage) FinishSplitOrder(SplitPaymentViewModel input)
        {
            Order? order = _paymentRepository.GetOpenOrderByTable(input.TableId);
            if (order == null)
                return (false, "No open order for this table.");

            OrderPaymentViewModel summary = BuildViewModel(order);

            if (input.Payments == null || input.Payments.Count == 0)
                return (false, "At least one payment is required.");

            decimal sumPaid = input.Payments.Sum(p => p.AmountPaid);
            if (sumPaid < summary.TotalInclVat)
            {
                decimal remaining = summary.TotalInclVat - sumPaid;
                return (false, $"Payments only cover €{sumPaid:0.00} of €{summary.TotalInclVat:0.00}. " +
                               $"Remaining to pay: €{remaining:0.00}");
            }

            List<Payment> payments = new List<Payment>();

            foreach (var person in input.Payments)
            {
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


        // --- private helpers ---

       
        private decimal CalculateTip(FinishOrderViewModel input, decimal billTotal)
        {
            decimal tip;

            if (input.TipAmount > 0m)
                tip = input.TipAmount;
            else
                tip = input.AmountPaid - billTotal;

            if (tip < 0m) tip = 0m;

            return Math.Round(tip, 2);
        }

        private List<PersonPaymentViewModel> BuildEqualSplitRows(decimal total, int numberOfPeople)
        {
            decimal share = Math.Round(total / numberOfPeople, 2);
            decimal lastShare = total - share * (numberOfPeople - 1);

            List<PersonPaymentViewModel> rows = new List<PersonPaymentViewModel>();
            for (int i = 0; i < numberOfPeople; i++)
            {
                rows.Add(new PersonPaymentViewModel
                {
                    AmountPaid = i == numberOfPeople - 1 ? lastShare : share,
                    PaymentMethod = PaymentMethod.Cash
                });
            }

            return rows;
        }

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