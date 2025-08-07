using AGROPURE.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.Entities
{
    public class Quote
    {
        public int Id { get; set; }
        public int? UserId { get; set; } // ACTUALIZADO: Nullable para cotizaciones públicas
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? CustomerPhone { get; set; }

        [MaxLength(500)]
        public string? CustomerAddress { get; set; }

        [MaxLength(200)]
        public string? CustomerCompany { get; set; } // NUEVO

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        public QuoteStatus Status { get; set; } = QuoteStatus.Pending;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(1000)]
        public string? AdminNotes { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResponseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public bool IsPublicQuote { get; set; } = false; // NUEVO

        // Navigation properties
        public User? User { get; set; } // ACTUALIZADO: Nullable
        public Product Product { get; set; } = null!;
    }
}
