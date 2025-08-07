using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGROPURE.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? DetailedDescription { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(1000)]
        public string? TechnicalSpecs { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<ProductMaterial> Materials { get; set; } = new List<ProductMaterial>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductFaq> Faqs { get; set; } = new List<ProductFaq>(); // NUEVO
    }
}
