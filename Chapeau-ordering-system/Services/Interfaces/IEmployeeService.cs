using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Services.Interfaces
{
    public interface IEmployeeService
    {
        Employee? Login(string username, string password);

        // Management
        List<Employee> GetAllEmployees();
        void AddEmployee(Employee employee, string plainPassword);
        void UpdateEmployee(Employee employee);
        void SetEmployeeActive(int employeeId, bool isActive);
    }
}
