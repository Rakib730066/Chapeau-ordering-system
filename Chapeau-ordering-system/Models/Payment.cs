using Chapeau_ordering_system.Models.Enums;

namespace Chapeau_ordering_system.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }          
        public decimal TipAmount { get; set; }
        public decimal VatLowAmount { get; set; }   
        public decimal VatHighAmount { get; set; }   
        public PaymentMethod PaymentMethod { get; set; }
        public string? Feedback { get; set; }
        public DateTime PaidAt { get; set; }
    }
}