using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.Entities
{
    public class Purchase
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int MaterialId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PurchaseNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveryDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public Supplier Supplier { get; set; } = null!;
        public Material Material { get; set; } = null!;
    }
}