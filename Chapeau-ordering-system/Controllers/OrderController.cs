using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;



namespace Chapeau_ordering_system.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IOrderService _orderService;

        private const string SessionOrderId = "CurrentOrderId";

        public OrderController(IMenuItemService menuItemService, IOrderService orderService)
        {
            _menuItemService = menuItemService;
            _orderService = orderService;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartOrder(int tableId)
        {
            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                int orderId = _orderService.StartOrder(tableId, employeeId.Value);

                HttpContext.Session.SetInt32(SessionOrderId, orderId);

                return RedirectToAction("TakeOrder", new { tableId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Menu", new { tableId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoadOrder(int tableId)
        {
            Order? order = _orderService.GetOpenOrders()
                .FirstOrDefault(o => o.Table?.TableId == tableId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "No open order found for this table.";
                return RedirectToAction("Menu", new { tableId });
            }

            HttpContext.Session.SetInt32(SessionOrderId, order.OrderId);

            return RedirectToAction("TakeOrder", new { tableId });
        }


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

        [HttpGet]
        public IActionResult TakeOrder(int tableId, MenuItemType? type, CourseType? course)
        {
            int orderId = HttpContext.Session.GetInt32(SessionOrderId) ?? 0;

            if (orderId == 0)
            {
                return RedirectToAction("Menu", new { tableId, type, course });
            }

            List<OrderItem> currentItems = _orderService.GetItemsByOrderId(orderId);

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
                ConfirmationMessage = TempData["ConfirmMessage"] as string,
                ErrorMessage = TempData["ErrorMessage"] as string
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(int menuItemId, int tableId, MenuItemType? type, CourseType? course)
        {
            int orderId = HttpContext.Session.GetInt32(SessionOrderId) ?? 0;

            if (orderId == 0)
            {
                TempData["ErrorMessage"] = "Please start an order before adding items.";
                return RedirectToAction("Menu", new { tableId, type = (int?)type, course = (int?)course });
            }

            try
            {
                _orderService.AddItemToOrder(orderId, menuItemId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("TakeOrder", new { tableId, type = (int?)type, course = (int?)course });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveOrder(int tableId)
        {
            int orderId = HttpContext.Session.GetInt32(SessionOrderId) ?? 0;

            if (orderId == 0)
            {
                TempData["ErrorMessage"] = "No active order found.";
                return RedirectToAction("Menu", new { tableId });
            }

            HttpContext.Session.Remove(SessionOrderId);
            TempData["ConfirmMessage"] = "Order sent successfully.";

            return RedirectToAction("Index", "RestaurantOverview");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IncreaseItem(int orderItemId, int currentQuantity, int tableId)
        {
            try
            {
                _orderService.IncreaseItemQuantity(orderItemId, currentQuantity);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("TakeOrder", new { tableId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DecreaseItem(int orderItemId, int currentQuantity, int tableId)
        {
            try
            {
                _orderService.DecreaseItemQuantity(orderItemId, currentQuantity);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("TakeOrder", new { tableId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateComment(int orderItemId, string? comment, int tableId)
        {
            try
            {
                _orderService.UpdateItemComment(orderItemId, comment);
                TempData["ConfirmMessage"] = "Comment updated successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("TakeOrder", new { tableId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int orderItemId, int tableId)
        {
            try
            {
                _orderService.RemoveItem(orderItemId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("TakeOrder", new { tableId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int orderId, int tableId)
        {
            try
            {
                _orderService.CancelOrder(orderId, tableId);

                HttpContext.Session.Remove(SessionOrderId);
                TempData["ConfirmMessage"] = "Order cancelled successfully.";

                return RedirectToAction("Index", "RestaurantOverview");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction("TakeOrder", new { tableId });
            }
        }
    }
}