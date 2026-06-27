using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Repositories.Interfaces;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class ManagementController : Controller
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IEmployeeService _employeeService;
        private readonly IPaymentRepository _paymentRepository;

        public ManagementController(
            IMenuItemService menuItemService,
            IEmployeeService employeeService,
            IPaymentRepository paymentRepository)
        {
            _menuItemService    = menuItemService;
            _employeeService    = employeeService;
            _paymentRepository  = paymentRepository;
        }

        private bool IsManager()
        {
            var role = HttpContext.Session.GetString("EmployeeRole");
            return role == EmployeeRole.Manager.ToString();
        }

        // ─── Menu Items ─────────────────────────────────────────────────────────

        public IActionResult MenuItems(MenuItemType? type, CardType? card)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");

            var items = _menuItemService.GetAllMenuItems();

            if (type.HasValue) items = items.Where(i => i.Type == type.Value).ToList();
            if (card.HasValue) items = items.Where(i => i.Card == card.Value).ToList();

            ViewBag.FilterType = type;
            ViewBag.FilterCard = card;
            return View(items);
        }

        [HttpGet]
        public IActionResult CreateMenuItem()
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            return View(new MenuItemFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateMenuItem(MenuItemFormViewModel vm)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View(vm);

            var item = new MenuItem
            {
                Name     = vm.Name,
                Price    = vm.Price,
                Type     = vm.Type,
                Course   = vm.Course,
                Card     = vm.Card,
                VatRate  = vm.VatRate,
                Stock    = vm.Stock,
                IsActive = vm.IsActive
            };

            _menuItemService.AddMenuItem(item);
            TempData["ConfirmMessage"] = $"Menu item '{vm.Name}' created.";
            return RedirectToAction(nameof(MenuItems));
        }

        [HttpGet]
        public IActionResult EditMenuItem(int id)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            var item = _menuItemService.GetMenuItemById(id);
            if (item == null) return NotFound();

            var vm = new MenuItemFormViewModel
            {
                MenuItemId = item.MenuItemId,
                Name       = item.Name,
                Price      = item.Price,
                Type       = item.Type,
                Course     = item.Course,
                Card       = item.Card,
                VatRate    = item.VatRate,
                Stock      = item.Stock,
                IsActive   = item.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMenuItem(MenuItemFormViewModel vm)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View(vm);

            var item = new MenuItem
            {
                MenuItemId = vm.MenuItemId,
                Name       = vm.Name,
                Price      = vm.Price,
                Type       = vm.Type,
                Course     = vm.Course,
                Card       = vm.Card,
                VatRate    = vm.VatRate,
                Stock      = vm.Stock,
                IsActive   = vm.IsActive
            };

            _menuItemService.UpdateMenuItem(item);
            TempData["ConfirmMessage"] = $"Menu item '{vm.Name}' updated.";
            return RedirectToAction(nameof(MenuItems));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleMenuItemActive(int id, bool isActive)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            _menuItemService.SetMenuItemActive(id, isActive);
            TempData["ConfirmMessage"] = isActive ? "Item activated." : "Item deactivated.";
            return RedirectToAction(nameof(MenuItems));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStock(int id, int newStock)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            if (newStock < 0) { TempData["ErrorMessage"] = "Stock cannot be negative."; return RedirectToAction(nameof(MenuItems)); }
            _menuItemService.UpdateStock(id, newStock);
            TempData["ConfirmMessage"] = "Stock updated.";
            return RedirectToAction(nameof(MenuItems));
        }

        // ─── Employees ──────────────────────────────────────────────────────────

        public IActionResult Employees()
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            var employees = _employeeService.GetAllEmployees();
            return View(employees);
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            return View(new EmployeeFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEmployee(EmployeeFormViewModel vm)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "Password is required for a new employee.");
            if (!ModelState.IsValid) return View(vm);

            var employee = new Employee
            {
                Username  = vm.Username,
                FirstName = vm.FirstName,
                LastName  = vm.LastName,
                Email     = vm.Email ?? string.Empty,
                Role      = vm.Role,
                IsActive  = vm.IsActive
            };

            _employeeService.AddEmployee(employee, vm.Password!);
            TempData["ConfirmMessage"] = $"Employee '{vm.FirstName} {vm.LastName}' created.";
            return RedirectToAction(nameof(Employees));
        }

        [HttpGet]
        public IActionResult EditEmployee(int id)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            var emp = _employeeService.GetAllEmployees().FirstOrDefault(e => e.EmployeeId == id);
            if (emp == null) return NotFound();

            var vm = new EmployeeFormViewModel
            {
                EmployeeId = emp.EmployeeId,
                Username   = emp.Username,
                FirstName  = emp.FirstName,
                LastName   = emp.LastName,
                Email      = emp.Email,
                Role       = emp.Role,
                IsActive   = emp.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEmployee(EmployeeFormViewModel vm)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            // Password is optional on edit — clear its validation if empty
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.Remove("Password");
            if (!ModelState.IsValid) return View(vm);

            var employee = new Employee
            {
                EmployeeId = vm.EmployeeId,
                Username   = vm.Username,
                FirstName  = vm.FirstName,
                LastName   = vm.LastName,
                Email      = vm.Email,
                Role       = vm.Role,
                IsActive   = vm.IsActive
            };

            _employeeService.UpdateEmployee(employee);

            // If a new password was supplied, hash and save it
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);
                _employeeService.UpdateEmployee(employee);
            }

            TempData["ConfirmMessage"] = $"Employee '{vm.FirstName} {vm.LastName}' updated.";
            return RedirectToAction(nameof(Employees));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleEmployeeActive(int id, bool isActive)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");
            _employeeService.SetEmployeeActive(id, isActive);
            TempData["ConfirmMessage"] = isActive ? "Employee activated." : "Employee deactivated.";
            return RedirectToAction(nameof(Employees));
        }

        // ─── Financial Overview ─────────────────────────────────────────────────

        [HttpGet]
        public IActionResult FinancialOverview(DateTime? startDate, DateTime? endDate)
        {
            if (!IsManager()) return RedirectToAction("Login", "Account");

            DateTime start = startDate ?? DateTime.Today.AddDays(-30);
            DateTime end   = endDate   ?? DateTime.Today;

            var vm = _paymentRepository.GetFinancialOverview(start, end);
            return View(vm);
        }
    }
}
