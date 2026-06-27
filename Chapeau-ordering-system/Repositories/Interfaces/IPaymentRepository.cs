using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Order? GetOpenOrderByTable(int tableId);
        void FinishOrder(Payment payment);
        void FinishOrderWithSplit(List<Payment> payments);

        // Management - financial overview
        FinancialOverviewViewModel GetFinancialOverview(DateTime startDate, DateTime endDate);
    }
}
