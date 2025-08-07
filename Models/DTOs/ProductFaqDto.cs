using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.DTOs
{
    public class ProductFaqDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductFaqDto
    {
        [Required]
        [MaxLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Answer { get; set; } = string.Empty;
    }
}
