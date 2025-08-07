using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.DTOs
{
    public class PurchaseDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string PurchaseNumber { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreatePurchaseDto
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitCost { get; set; }

        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdatePurchaseDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitCost { get; set; }

        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class InventoryDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal TotalQuantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastPurchaseDate { get; set; }
    }
}
