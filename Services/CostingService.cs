using AGROPURE.Data;

namespace AGROPURE.Services
{
    public class CostingService
    {
        private readonly AgroContext _context;
        private const decimal LABOR_COST_PERCENTAGE = 0.30m; // 30% del costo de materiales
        private const decimal OVERHEAD_PERCENTAGE = 0.20m; // 20% del costo total
        private const decimal PROFIT_MARGIN = 0.25m; // 25% de ganancia

        public CostingService(AgroContext context)
        {
            _context = context;
        }

        public async Task<CostCalculationDto> CalculateProductCostAsync(int productId, int quantity)
        {
            var product = await _context.Products
                .Include(p => p.Materials)
                    .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Producto no encontrado");
            }

            // Costo de materiales
            var materialCost = product.Materials.Sum(pm => pm.Quantity * pm.Material.UnitCost);

            // Costo de mano de obra
            var laborCost = materialCost * LABOR_COST_PERCENTAGE;

            // Costo indirecto
            var overheadCost = (materialCost + laborCost) * OVERHEAD_PERCENTAGE;

            // Costo total de producción
            var totalProductionCost = materialCost + laborCost + overheadCost;

            // Precio con margen de ganancia
            var unitPriceWithProfit = totalProductionCost * (1 + PROFIT_MARGIN);

            // Descuentos por volumen
            var volumeDiscount = CalculateVolumeDiscount(quantity);
            var finalUnitPrice = unitPriceWithProfit * (1 - volumeDiscount);

            return new CostCalculationDto
            {
                ProductId = productId,
                Quantity = quantity,
                MaterialCost = materialCost,
                LaborCost = laborCost,
                OverheadCost = overheadCost,
                TotalProductionCost = totalProductionCost,
                ProfitMargin = PROFIT_MARGIN,
                VolumeDiscount = volumeDiscount,
                UnitPrice = Math.Round(finalUnitPrice, 2),
                TotalCost = Math.Round(finalUnitPrice * quantity, 2)
            };
        }

        private decimal CalculateVolumeDiscount(int quantity)
        {
            return quantity switch
            {
                >= 10 => 0.15m, // 15% descuento para 10+ unidades
                >= 5 => 0.10m,  // 10% descuento para 5+ unidades
                >= 3 => 0.05m,  // 5% descuento para 3+ unidades
                _ => 0m         // Sin descuento
            };
        }
    }

    public class CostCalculationDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal OverheadCost { get; set; }
        public decimal TotalProductionCost { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal VolumeDiscount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
    }
}