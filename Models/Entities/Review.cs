using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }

        public int Rating { get; set; } // 1-5 stars

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}