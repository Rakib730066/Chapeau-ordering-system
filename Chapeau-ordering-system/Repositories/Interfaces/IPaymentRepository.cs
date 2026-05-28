using Chapeau_ordering_system.Models;
namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Order? GetOpenOrderByTable(int tableId);
    }
}