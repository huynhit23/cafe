using System.ComponentModel.DataAnnotations.Schema;

namespace cafe.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string? PaymentMethod { get; set; }

        public DateTime PaymentDate { get; set; }

        public string? TransactionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Status { get; set; }

        public Order? Order { get; set; }
    }
}
