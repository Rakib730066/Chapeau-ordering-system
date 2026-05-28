using Chapeau_ordering_system.Models.Enums;
using Chapeau_ordering_system.Services.Interfaces;
using Chapeau_ordering_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chapeau_ordering_system.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMenuItemService _menuItemService;

        public OrderController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        // Show the menu for a specific table, with optional filters
        // tableId: which table the waiter is taking an order for
        // type: filter by card (Drink=2 / Food=1), null = show all
        // course: filter by course (Starter, Main, etc.), null = show all
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
    }
}