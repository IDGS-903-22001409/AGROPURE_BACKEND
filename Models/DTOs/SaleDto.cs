using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int? QuoteId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Navigation properties
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
    }

    public class CreateSaleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? QuoteId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public string? Notes { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}
