using Chapeau_ordering_system.Mappers;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class ManagementController : BaseController
    {
        private readonly IMenuItemService    _menuItemService;
        private readonly IEmployeeService    _employeeService;
        private readonly IPaymentRepository  _paymentRepository;

        public ManagementController(IMenuItemService menuItemService, IEmployeeService employeeService, IPaymentRepository paymentRepository)
        {
            _menuItemService   = menuItemService;
            _employeeService   = employeeService;
            _paymentRepository = paymentRepository;
        }

        // ─── Menu Items ──────────────────────────────────────────────────────────

        public IActionResult MenuItems(MenuItemType? type, CardType? card)
        {
            if (ManagerGuard() is { } r) return r;
            var items = _menuItemService.GetAllMenuItems();
            if (type.HasValue) items = items.Where(i => i.Type == type.Value).ToList();
            if (card.HasValue) items = items.Where(i => i.Card == card.Value).ToList();
            items = items.OrderBy(i => i.Card).ThenBy(i => i.Course).ThenBy(i => i.Name).ToList();
            ViewBag.FilterType = type;
            ViewBag.FilterCard = card;
            return View(items);
        }

        [HttpGet]
        public IActionResult CreateMenuItem()
        {
            if (ManagerGuard() is { } r) return r;
            return View(new MenuItemFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreateMenuItem(MenuItemFormViewModel vm)
        {
            if (ManagerGuard() is { } r) return r;
            if (!ModelState.IsValid) return View(vm);
            _menuItemService.AddMenuItem(MenuItemMapper.ToModel(vm));
            SetSuccess($"Menu item '{vm.Name}' created.");
            return RedirectToAction(nameof(MenuItems));
        }

        [HttpGet]
        public IActionResult EditMenuItem(int id)
        {
            if (ManagerGuard() is { } r) return r;
            var item = _menuItemService.GetMenuItemById(id);
            if (item == null) return NotFound();
            return View(MenuItemMapper.ToViewModel(item));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditMenuItem(MenuItemFormViewModel vm)
        {
            if (ManagerGuard() is { } r) return r;
            if (!ModelState.IsValid) return View(vm);
            _menuItemService.UpdateMenuItem(MenuItemMapper.ToModel(vm));
            SetSuccess($"Menu item '{vm.Name}' updated.");
            return RedirectToAction(nameof(MenuItems));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ToggleMenuItemActive(int id, bool isActive)
        {
            if (ManagerGuard() is { } r) return r;
            _menuItemService.SetMenuItemActive(id, isActive);
            SetSuccess(isActive ? "Item activated." : "Item deactivated.");
            return RedirectToAction(nameof(MenuItems));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateStock(int id, int newStock)
        {
            if (ManagerGuard() is { } r) return r;
            if (newStock < 0) { SetError("Stock cannot be negative."); return RedirectToAction(nameof(MenuItems)); }
            _menuItemService.UpdateStock(id, newStock);
            SetSuccess("Stock updated.");
            return RedirectToAction(nameof(MenuItems));
        }

        // ─── Employees ───────────────────────────────────────────────────────────

        public IActionResult Employees()
        {
            if (ManagerGuard() is { } r) return r;
            return View(_employeeService.GetAllEmployees().OrderBy(e => e.LastName).ToList());
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            if (ManagerGuard() is { } r) return r;
            return View(new EmployeeFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreateEmployee(EmployeeFormViewModel vm)
        {
            if (ManagerGuard() is { } r) return r;
            if (string.IsNullOrWhiteSpace(vm.Password)) ModelState.AddModelError("Password", "Password is required for a new employee.");
            if (!ModelState.IsValid) return View(vm);
            _employeeService.AddEmployee(EmployeeMapper.ToModel(vm), vm.Password!);
            SetSuccess($"Employee '{vm.FirstName} {vm.LastName}' created.");
            return RedirectToAction(nameof(Employees));
        }

        [HttpGet]
        public IActionResult EditEmployee(int id)
        {
            if (ManagerGuard() is { } r) return r;
            var emp = _employeeService.GetAllEmployees().FirstOrDefault(e => e.EmployeeId == id);
            if (emp == null) return NotFound();
            return View(EmployeeMapper.ToViewModel(emp));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditEmployee(EmployeeFormViewModel vm)
        {
            if (ManagerGuard() is { } r) return r;
            if (string.IsNullOrWhiteSpace(vm.Password)) ModelState.Remove("Password");
            if (!ModelState.IsValid) return View(vm);
            var employee = EmployeeMapper.ToModel(vm);
            _employeeService.UpdateEmployee(employee);
            if (!string.IsNullOrWhiteSpace(vm.Password)) UpdateEmployeePassword(employee, vm.Password!);
            SetSuccess($"Employee '{vm.FirstName} {vm.LastName}' updated.");
            return RedirectToAction(nameof(Employees));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ToggleEmployeeActive(int id, bool isActive)
        {
            if (ManagerGuard() is { } r) return r;
            _employeeService.SetEmployeeActive(id, isActive);
            SetSuccess(isActive ? "Employee activated." : "Employee deactivated.");
            return RedirectToAction(nameof(Employees));
        }

        // ─── Financial Overview ──────────────────────────────────────────────────

        [HttpGet]
        public IActionResult FinancialOverview(DateTime? startDate, DateTime? endDate)
        {
            if (ManagerGuard() is { } r) return r;
            var vm = _paymentRepository.GetFinancialOverview(startDate ?? DateTime.Today.AddDays(-30), endDate ?? DateTime.Today);
            return View(vm);
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private void UpdateEmployeePassword(Models.Employee employee, string password)
        {
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _employeeService.UpdateEmployee(employee);
        }
    }
}
