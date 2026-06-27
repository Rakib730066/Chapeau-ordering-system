namespace Chapeau_ordering_system.ViewModels
{
    public class FinancialOverviewViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TotalTip { get; set; }
        public decimal TotalVatLow { get; set; }
        public decimal TotalVatHigh { get; set; }

        public decimal LunchRevenue { get; set; }
        public decimal DinnerRevenue { get; set; }
        public decimal DrinksRevenue { get; set; }

        public int TotalOrders { get; set; }

        public List<DailyRevenueLine> DailyBreakdown { get; set; } = new();
    }

    public class DailyRevenueLine
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Tip { get; set; }
        public int Orders { get; set; }
    }
}
