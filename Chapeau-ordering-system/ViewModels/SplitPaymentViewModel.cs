using Chapeau_ordering_system.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chapeau_ordering_system.ViewModels
{
    public enum SplitMode
    {
        Equal = 1,
        Custom = 2
    }

    public class SplitPaymentViewModel
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;

        // Bill summary
        public decimal TotalToPay { get; set; }
        public decimal VatLow { get; set; }
        public decimal VatHigh { get; set; }
        public SplitMode Mode { get; set; } = SplitMode.Equal;

        [Range(2, 20, ErrorMessage = "Number of people must be between 2 and 20.")]
        public int NumberOfPeople { get; set; } = 2;
        public List<PersonPaymentViewModel> Payments { get; set; } = new List<PersonPaymentViewModel>();

        public decimal SumSoFar      => Payments?.Sum(p => p.AmountPaid) ?? 0m;
        public decimal Remaining     => Math.Max(TotalToPay - SumSoFar, 0m);
        public decimal PerPersonShare => NumberOfPeople > 0 ? Math.Round(TotalToPay / NumberOfPeople, 2) : 0m;
    }

    public class PersonPaymentViewModel
    {
        [Range(0.00, 10000.00, ErrorMessage = "Amount must be 0 or more.")]
        public decimal AmountPaid { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string? Feedback { get; set; }
    }
}