using Chapeau_ordering_system.ViewModels;
namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IPaymentService
    {
        OrderPaymentViewModel? GetOrderForPayment(int tableId);
        FinishOrderViewModel? GetFinishOrderViewModel(int tableId);
        void FinishOrder(FinishOrderViewModel input);
    }
}