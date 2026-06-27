namespace Chapeau_ordering_system.ViewModels
{
    public class OrderPaymentViewModel
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;

        public List<OrderLineViewModel> Lines { get; set; } = new List<OrderLineViewModel>();

        public decimal VatLow { get; set; }       
        public decimal VatHigh { get; set; }       
        public decimal TotalInclVat { get; set; }  
    }

    public class OrderLineViewModel
    {
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
        public decimal VatRate { get; set; }
    }
}