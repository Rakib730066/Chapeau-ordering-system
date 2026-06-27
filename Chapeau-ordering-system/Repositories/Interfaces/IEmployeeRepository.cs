using Chapeau_ordering_system.Models;

namespace Chapeau_ordering_system.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Employee? GetByUsername(string username);

        // Management
        List<Employee> GetAll();
        void Add(Employee employee);
        void Update(Employee employee);
        void SetActive(int employeeId, bool isActive);
    }
}
