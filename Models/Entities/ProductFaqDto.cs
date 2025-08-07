using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.Entities
{
    public class ProductFaq
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Answer { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}