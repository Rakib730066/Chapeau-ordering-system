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
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
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
            if (employee.Role.ToString() == "Bar" || employee.Role.ToString() == "Kitchen")
            {
                return RedirectToAction("Index", "BarKitchen");
            }
            else
            {
                return RedirectToAction("Index", "RestaurantOverview");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}