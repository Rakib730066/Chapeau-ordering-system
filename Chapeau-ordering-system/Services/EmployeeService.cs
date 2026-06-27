using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;

namespace Chapeau_ordering_system.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public Employee? Login(string username, string password)
        {
            var employee = _employeeRepository.GetByUsername(username);
            if (employee == null || string.IsNullOrWhiteSpace(employee.PasswordHash))
                return null;
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash) ? employee : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Management
        public List<Employee> GetAllEmployees() => _employeeRepository.GetAll();

        public void AddEmployee(Employee employee, string plainPassword)
        {
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            _employeeRepository.Add(employee);
        }

        public void UpdateEmployee(Employee employee) => _employeeRepository.Update(employee);

        public void SetEmployeeActive(int employeeId, bool isActive)
            => _employeeRepository.SetActive(employeeId, isActive);
    }
}
