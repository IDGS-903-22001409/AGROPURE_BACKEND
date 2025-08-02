using System.ComponentModel.DataAnnotations.Schema;


namespace AGROPURE.Models.Entities
{
    public class ProductMaterial
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int MaterialId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
        public Material Material { get; set; } = null!;
    }
}