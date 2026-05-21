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
    }
}
