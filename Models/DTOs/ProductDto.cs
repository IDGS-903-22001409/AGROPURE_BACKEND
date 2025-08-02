using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? DetailedDescription { get; set; }
        public string? ImageUrl { get; set; }
        public decimal BasePrice { get; set; }
        public string? Category { get; set; }
        public string? TechnicalSpecs { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ProductMaterialDto> Materials { get; set; } = new List<ProductMaterialDto>();
        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public double AverageRating { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? DetailedDescription { get; set; }
        public string? ImageUrl { get; set; }
        public decimal BasePrice { get; set; }
        public string? Category { get; set; }
        public string? TechnicalSpecs { get; set; }

        public List<CreateProductMaterialDto> Materials { get; set; } = new List<CreateProductMaterialDto>();
    }

    public class ProductMaterialDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal TotalCost => Quantity * UnitCost;
    }

    public class CreateProductMaterialDto
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
    }
}