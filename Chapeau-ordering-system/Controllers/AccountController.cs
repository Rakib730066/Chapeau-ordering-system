using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IEmployeeService _employeeService;
        public AccountController(IEmployeeService employeeService) => _employeeService = employeeService;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var employee = _employeeService.Login(model.Username, model.Password);
            if (employee == null)
            {
                ModelState.AddModelError("", "Invalid username or password. Please try again.");
                return View(model);
            }
            StoreEmployeeSession(employee);
            return RedirectAfterLogin(employee.Role);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return LoginRedirect();
        }

        private void StoreEmployeeSession(Employee e)
        {
            HttpContext.Session.SetInt32("EmployeeId", e.EmployeeId);
            HttpContext.Session.SetString("EmployeeName", e.FullName);
            HttpContext.Session.SetString("EmployeeRole", e.Role.ToString());
        }

        private IActionResult RedirectAfterLogin(EmployeeRole role) =>
            role is EmployeeRole.Bar or EmployeeRole.Kitchen
                ? RedirectToAction("Index", "BarKitchen")
                : OverviewRedirect();
    }
}
