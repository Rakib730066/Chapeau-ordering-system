using Chapeau_ordering_system.Models;
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
            if (order == null)
                return null;

            var viewModel = new OrderPaymentViewModel
            {
                OrderId = order.OrderId,
                TableId = order.Table!.TableId,
                TableNumber = order.Table.TableNumber
            };

            // Group order items by menu item so each appears once with summed quantity.
            var grouped = order.OrderItems
                .Where(oi => oi.Status != Models.Enums.OrderItemStatus.Cancelled)
                .GroupBy(oi => oi.MenuItem!.MenuItemId)
                .Select(g => new OrderLineViewModel
                {
                    MenuItemName = g.First().MenuItem!.Name,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.First().MenuItem!.Price,
                    VatRate = g.First().MenuItem!.VatRate
                })
                .ToList();

            viewModel.Lines = grouped;

            decimal vatLow = 0m;
            decimal vatHigh = 0m;
            decimal total = 0m;

            foreach (var line in grouped)
            {
                total += line.LineTotal;

                // Dutch menu prices are VAT-INCLUSIVE.
                // VAT contained in a gross price = gross * (rate / (100 + rate))
                decimal vatPortion = line.LineTotal * (line.VatRate / (100m + line.VatRate));

                if (line.VatRate == 9m)
                    vatLow += vatPortion;
                else if (line.VatRate == 21m)
                    vatHigh += vatPortion;
            }

            viewModel.TotalInclVat = Math.Round(total, 2);
            viewModel.VatLow = Math.Round(vatLow, 2);
            viewModel.VatHigh = Math.Round(vatHigh, 2);

            return viewModel;
        }
    }
}