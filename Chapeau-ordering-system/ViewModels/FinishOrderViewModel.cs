using Chapeau_ordering_system.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chapeau_ordering_system.ViewModels
{
    public class FinishOrderViewModel
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;

        public decimal TotalToPay { get; set; }
        public decimal VatLow { get; set; }
        public decimal VatHigh { get; set; }

        [Required]
        [Range(0.01, 10000.00, ErrorMessage = "Total paid must be greater than 0.")]
        public decimal AmountPaid { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string? Feedback { get; set; }
    }
}