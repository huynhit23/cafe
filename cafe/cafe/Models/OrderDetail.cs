using System.ComponentModel.DataAnnotations.Schema;

namespace cafe.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Size { get; set; } = "Regular";
        public string SugarLevel { get; set; } = "100%";
        public string IceLevel { get; set; } = "100%";

        public Order? Order { get; set; }

        public Product? Product { get; set; }
    }

}
