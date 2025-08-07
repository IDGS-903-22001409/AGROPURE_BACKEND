namespace AGROPURE.Models.DTOs
{
    public class MaterialDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitCost { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMaterialDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitCost { get; set; }

        [Required]
        [MaxLength(20)]
        public string Unit { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }
    }
}
