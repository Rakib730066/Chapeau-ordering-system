using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IEmployeeService
    {
        Employee? Login(string username, string password);
    }
}