using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chapeau_ordering_system.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IOrderService _orderService;

        // Session keys
        private const string SessionOrderId = "CurrentOrderId";
        private const string SessionOrderItems = "CurrentOrderItems";

        public OrderController(IMenuItemService menuItemService, IOrderService orderService)
        {
            _menuItemService = menuItemService;
            _orderService = orderService;
        }

        // Sprint 1 — Show the menu for a table with optional filters
        [HttpGet]
        public IActionResult Menu(int tableId, MenuItemType? type, CourseType? course)
        {
            MenuViewModel viewModel = new MenuViewModel
            {
                MenuItems = _menuItemService.GetFilteredMenuItems(type, course),
                ActiveType = type,
                ActiveCourse = course,
                TableId = tableId
            };
            return View(viewModel);
        }

        // Sprint 2 — Start a new order for a table and go to TakeOrder page
        [HttpPost]
        public IActionResult StartOrder(int tableId)
        {
            int employeeId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
            int orderId = _orderService.StartOrder(tableId, employeeId);

            // Store orderId and empty item list in session
            HttpContext.Session.SetInt32(SessionOrderId, orderId);
            HttpContext.Session.SetString(SessionOrderItems, JsonSerializer.Serialize(new List<OrderItem>()));

            return RedirectToAction("TakeOrder", new { tableId });
        }

        // Sprint 2 — Show the TakeOrder page (menu + running order list)
        [HttpGet]
        public IActionResult TakeOrder(int tableId, MenuItemType? type, CourseType? course)
        {
            int orderId = HttpContext.Session.GetInt32(SessionOrderId) ?? 0;
            List<OrderItem> currentItems = GetItemsFromSession();

            TakeOrderViewModel viewModel = new TakeOrderViewModel
            {
                OrderId = orderId,
                CurrentItems = currentItems,
                Menu = new MenuViewModel
                {
                    MenuItems = _menuItemService.GetFilteredMenuItems(type, course),
                    ActiveType = type,
                    ActiveCourse = course,
                    TableId = tableId
                },
                ConfirmationMessage = TempData["ConfirmMessage"] as string
            };

            return View(viewModel);
        }

        // Sprint 2 — Add a menu item to the in-progress order (increments if already exists)
        [HttpPost]
        [HttpPost]
        public IActionResult AddItem(int menuItemId, int tableId, MenuItemType? type, CourseType? course)
        {
            List<OrderItem> currentItems = GetItemsFromSession();

            OrderItem? existing = currentItems.FirstOrDefault(i => i.MenuItem!.MenuItemId == menuItemId);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                MenuItem? menuItem = _menuItemService.GetMenuItemById(menuItemId);
                if (menuItem != null)
                {
                    currentItems.Add(new OrderItem
                    {
                        MenuItem = menuItem,
                        Quantity = 1,
                        Status = OrderItemStatus.Ordered,
                        OrderTime = DateTime.Now
                    });
                }
            }

            SaveItemsToSession(currentItems);

            // Pass the filter values back so the page stays on the same filter
            return RedirectToAction("TakeOrder", new { tableId, type = (int?)type, course = (int?)course });
        }

        // Sprint 2 — Save the order to the database and return to overview
        [HttpPost]
        public IActionResult SaveOrder(int tableId)
        {
            int orderId = HttpContext.Session.GetInt32(SessionOrderId) ?? 0;
            List<OrderItem> currentItems = GetItemsFromSession();

            if (orderId > 0 && currentItems.Any())
            {
                _orderService.SaveOrder(orderId, currentItems);

                // Clear session after saving
                HttpContext.Session.Remove(SessionOrderId);
                HttpContext.Session.Remove(SessionOrderItems);

                TempData["ConfirmMessage"] = "Order saved successfully!";
            }

            return RedirectToAction("Index", "RestaurantOverview");
        }

        // Helper — read current order items from session
        private List<OrderItem> GetItemsFromSession()
        {
            string? json = HttpContext.Session.GetString(SessionOrderItems);
            if (string.IsNullOrEmpty(json))
                return new List<OrderItem>();

            return JsonSerializer.Deserialize<List<OrderItem>>(json) ?? new List<OrderItem>();
        }

        // Helper — save current order items to session
        private void SaveItemsToSession(List<OrderItem> items)
        {
            HttpContext.Session.SetString(SessionOrderItems, JsonSerializer.Serialize(items));
        }
    }
}