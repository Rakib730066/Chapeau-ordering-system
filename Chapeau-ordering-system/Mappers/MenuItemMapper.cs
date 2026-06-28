using Chapeau_ordering_system.Models;
using Chapeau_ordering_system.ViewModels;

namespace Chapeau_ordering_system.Mappers
{
    public static class MenuItemMapper
    {
        public static MenuItem ToModel(MenuItemFormViewModel vm) => new()
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

        public static MenuItemFormViewModel ToViewModel(MenuItem item) => new()
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
    }
}
