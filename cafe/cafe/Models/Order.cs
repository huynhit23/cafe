using System.ComponentModel.DataAnnotations.Schema;

namespace cafe.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public string? ShippingAddress { get; set; }

        public string? PaymentStatus { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }

        public Payment? Payment { get; set; }
    }
}
