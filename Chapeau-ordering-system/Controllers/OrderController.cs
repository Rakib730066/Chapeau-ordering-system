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
                SetOrderSession(orderId);

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
            try
            {
                Order? order = _orderService.GetOrderByTableId(tableId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "No active order found for this table.";
                    return RedirectToAction("Menu", new { tableId });
                }
                SetOrderSession(order.OrderId);

                return RedirectToAction("TakeOrder", new { tableId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Menu", new { tableId });
            }
        }

        [HttpGet]
        public IActionResult Menu(int tableId, MenuItemType? type, CourseType? course, CardType? card)
        {
            MenuViewModel viewModel = new MenuViewModel
            {
                MenuItems = _menuItemService.GetFilteredMenuItems(type, course, card),
                ActiveType = type,
                ActiveCourse = course,
                ActiveCard = card,
                TableId = tableId
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult TakeOrder(int tableId, MenuItemType? type, CourseType? course, CardType? card)
        {
            int orderId = GetOrderSession();

            if (orderId == 0)
            {
                return RedirectToAction("Menu", new { tableId, type, course, card });
            }

            List<OrderItem> currentItems = _orderService.GetItemsByOrderId(orderId);

            TakeOrderViewModel viewModel = new TakeOrderViewModel
            {
                OrderId = orderId,
                CurrentItems = currentItems,
                Menu = new MenuViewModel
                {
                    MenuItems = _menuItemService.GetFilteredMenuItems(type, course, card),
                    ActiveType = type,
                    ActiveCourse = course,
                    ActiveCard = card,
                    TableId = tableId
                },
                ConfirmationMessage = TempData["ConfirmMessage"] as string,
                ErrorMessage = TempData["ErrorMessage"] as string
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(int menuItemId, int tableId, MenuItemType? type, CourseType? course, CardType? card)
        {
            int orderId = GetOrderSession();

            if (orderId == 0)
            {
                TempData["ErrorMessage"] = "Please start an order before adding items.";
                return RedirectToAction("Menu", new { tableId, type = (int?)type, course = (int?)course, card = (int?)card });
            }

            try
            {
                _orderService.AddItemToOrder(orderId, menuItemId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToTakeOrder(tableId, type, course, card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendOrder(int tableId)
        {
            int orderId = GetOrderSession();

            if (orderId == 0)
            {
                TempData["ErrorMessage"] = "No active order found.";
                return RedirectToAction("Menu", new { tableId });
            }

            try
            {
                ValidateAndSendOrder(orderId);
                ClearOrderSession();
                TempData["ConfirmMessage"] = "Order sent successfully to the kitchen.";
                return RedirectToAction("Index", "RestaurantOverview");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("TakeOrder", new { tableId });
            }
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
                ClearOrderSession();
                TempData["ConfirmMessage"] = "Order cancelled successfully.";

                return RedirectToAction("Index", "RestaurantOverview");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("TakeOrder", new { tableId });
            }
        }

        private void ValidateAndSendOrder(int orderId)
        {
            List<OrderItem> items = _orderService.GetItemsByOrderId(orderId);

            if (!items.Any())
                throw new InvalidOperationException("Order cannot be empty. Add items before sending.");

            _orderService.SaveOrder(orderId);
        }

        private IActionResult RedirectToTakeOrder(int tableId, MenuItemType? type = null, CourseType? course = null, CardType? card = null)
        {
            return RedirectToAction("TakeOrder", new { tableId, type = (int?)type, course = (int?)course, card = (int?)card });
        }

        private int GetOrderSession()
        {
            return HttpContext.Session.GetInt32(SessionOrderId) ?? 0;
        }

        private void SetOrderSession(int orderId)
        {
            HttpContext.Session.SetInt32(SessionOrderId, orderId);
        }

        private void ClearOrderSession()
        {
            HttpContext.Session.Remove(SessionOrderId);
        }
    }
}