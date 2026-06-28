using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Mappers
{
    public static class EmployeeMapper
    {
        public static Employee ToModel(EmployeeFormViewModel vm) => new()
        {
            EmployeeId = vm.EmployeeId,
            Username   = vm.Username,
            FirstName  = vm.FirstName,
            LastName   = vm.LastName,
            Email      = vm.Email ?? string.Empty,
            Role       = vm.Role,
            IsActive   = vm.IsActive
        };

        public static EmployeeFormViewModel ToViewModel(Employee emp) => new()
        {
            EmployeeId = emp.EmployeeId,
            Username   = emp.Username,
            FirstName  = emp.FirstName,
            LastName   = emp.LastName,
            Email      = emp.Email,
            Role       = emp.Role,
            IsActive   = emp.IsActive
        };
    }
}
