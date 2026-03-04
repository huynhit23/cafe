using System.ComponentModel.DataAnnotations;

namespace cafe.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [StringLength(300)]
        public string Title { get; set; }

        public string Content { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;
    }
}
