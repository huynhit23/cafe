using System.ComponentModel.DataAnnotations;

namespace cafe.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [StringLength(1000)]
        public string Content { get; set; }

        public int Rating { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ApplicationUser? User { get; set; }
    }
}
