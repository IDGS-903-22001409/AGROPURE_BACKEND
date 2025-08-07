using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AGROPURE.Models.Enums;
using AutoMapper;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PurchasesController : ControllerBase
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public PurchasesController(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<PurchaseDto>>> GetPurchases()
        {
            try
            {
                var purchases = await _context.Purchases
                    .Include(p => p.Supplier)
                    .Include(p => p.Material)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync();

                var purchaseDtos = purchases.Select(p => new PurchaseDto
                {
                    Id = p.Id,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.Name,
                    MaterialId = p.MaterialId,
                    MaterialName = p.Material.Name,
                    PurchaseNumber = p.PurchaseNumber,
                    Quantity = p.Quantity,
                    UnitCost = p.UnitCost,
                    TotalCost = p.TotalCost,
                    PurchaseDate = p.PurchaseDate,
                    DeliveryDate = p.DeliveryDate,
                    Notes = p.Notes,
                    Status = GetPurchaseStatus(p.DeliveryDate)
                }).ToList();

                return Ok(purchaseDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseDto>> GetPurchase(int id)
        {
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.Supplier)
                    .Include(p => p.Material)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchase == null)
                {
                    return NotFound(new { message = "Compra no encontrada" });
                }

                var purchaseDto = new PurchaseDto
                {
                    Id = purchase.Id,
                    SupplierId = purchase.SupplierId,
                    SupplierName = purchase.Supplier.Name,
                    MaterialId = purchase.MaterialId,
                    MaterialName = purchase.Material.Name,
                    PurchaseNumber = purchase.PurchaseNumber,
                    Quantity = purchase.Quantity,
                    UnitCost = purchase.UnitCost,
                    TotalCost = purchase.TotalCost,
                    PurchaseDate = purchase.PurchaseDate,
                    DeliveryDate = purchase.DeliveryDate,
                    Notes = purchase.Notes,
                    Status = GetPurchaseStatus(purchase.DeliveryDate)
                };

                return Ok(purchaseDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseDto>> CreatePurchase([FromBody] CreatePurchaseDto createDto)
        {
            try
            {
                // Verificar que el material y proveedor existan
                var material = await _context.Materials.FindAsync(createDto.MaterialId);
                var supplier = await _context.Suppliers.FindAsync(createDto.SupplierId);

                if (material == null || !material.IsActive)
                {
                    return BadRequest(new { message = "Material no válido" });
                }

                if (supplier == null || !supplier.IsActive)
                {
                    return BadRequest(new { message = "Proveedor no válido" });
                }

                var purchase = new Purchase
                {
                    SupplierId = createDto.SupplierId,
                    MaterialId = createDto.MaterialId,
                    PurchaseNumber = GeneratePurchaseNumber(),
                    Quantity = createDto.Quantity,
                    UnitCost = createDto.UnitCost,
                    TotalCost = createDto.Quantity * createDto.UnitCost,
                    PurchaseDate = DateTime.UtcNow,
                    DeliveryDate = createDto.DeliveryDate,
                    Notes = createDto.Notes
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                return await GetPurchase(purchase.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PurchaseDto>> UpdatePurchase(int id, [FromBody] UpdatePurchaseDto updateDto)
        {
            try
            {
                var purchase = await _context.Purchases.FindAsync(id);
                if (purchase == null)
                {
                    return NotFound(new { message = "Compra no encontrada" });
                }

                purchase.Quantity = updateDto.Quantity;
                purchase.UnitCost = updateDto.UnitCost;
                purchase.TotalCost = updateDto.Quantity * updateDto.UnitCost;
                purchase.DeliveryDate = updateDto.DeliveryDate;
                purchase.Notes = updateDto.Notes;

                await _context.SaveChangesAsync();
                return await GetPurchase(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePurchase(int id)
        {
            try
            {
                var purchase = await _context.Purchases.FindAsync(id);
                if (purchase == null)
                {
                    return NotFound(new { message = "Compra no encontrada" });
                }

                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Compra eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("inventory")]
        public async Task<ActionResult<List<InventoryDto>>> GetInventory()
        {
            try
            {
                var inventory = await _context.Purchases
                    .Include(p => p.Material)
                    .Include(p => p.Supplier)
                    .GroupBy(p => new { p.MaterialId, p.Material.Name, p.Material.Unit })
                    .Select(g => new InventoryDto
                    {
                        MaterialId = g.Key.MaterialId,
                        MaterialName = g.Key.Name,
                        Unit = g.Key.Unit,
                        TotalQuantity = g.Sum(p => p.Quantity),
                        AverageCost = g.Average(p => p.UnitCost),
                        TotalValue = g.Sum(p => p.TotalCost),
                        LastPurchaseDate = g.Max(p => p.PurchaseDate)
                    })
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string GeneratePurchaseNumber()
        {
            return $"PUR-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        }

        private string GetPurchaseStatus(DateTime? deliveryDate)
        {
            if (deliveryDate == null)
                return "Pendiente";

            return deliveryDate <= DateTime.Now ? "Entregado" : "En Tránsito";
        }
    }
}
