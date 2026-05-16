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
            Employee? employee = _employeeRepository.GetByUsername(username);

            if (employee == null)
                return null;

            // Guard against missing or invalid password hashes in the database.
            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                // Treat missing hash as authentication failure.
                return null;
            }

            bool passwordIsCorrect;
            try
            {
                passwordIsCorrect = BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash);
            }
            catch (ArgumentException)
            {
                // BCrypt throws ArgumentException when the stored hash/salt is invalid or empty.
                // Treat this as authentication failure rather than crashing the app.
                return null;
            }

            if (!passwordIsCorrect)
                return null;

            return employee;
        }
    }
}