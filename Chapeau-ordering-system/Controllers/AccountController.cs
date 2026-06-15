using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public AccountController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not load the login page: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var employee = _employeeService.Login(model.Username, model.Password);

                if (employee == null)
                {
                    ModelState.AddModelError("", "Invalid username or password. Please try again.");
                    return View(model);
                }

                HttpContext.Session.SetInt32("EmployeeId", employee.EmployeeId);
                HttpContext.Session.SetString("EmployeeName", employee.FullName);
                HttpContext.Session.SetString("EmployeeRole", employee.Role.ToString());

                // Redirect based on employee role
                if (employee.Role == EmployeeRole.Bar || employee.Role == EmployeeRole.Kitchen)
                    return RedirectToAction("Index", "BarKitchen");

                return RedirectToAction("Index", "RestaurantOverview");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Could not log in: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not log out: {ex.Message}";
                return RedirectToAction("Login");
            }
        }
    }
}
