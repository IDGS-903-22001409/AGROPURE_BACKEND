using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AGROPURE.Helpers;
using AGROPURE.Models.DTOs;
using AGROPURE.Services;
using AGROPURE.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SalesController : ControllerBase
    {
        private readonly SaleService _saleService;

        public SalesController(SaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SaleDto>>> GetAllSales()
        {
            try
            {
                var sales = await _saleService.GetAllSalesAsync();
                return Ok(sales);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDto>> GetSale(int id)
        {
            try
            {
                var sale = await _saleService.GetSaleByIdAsync(id);
                if (sale == null)
                {
                    return NotFound(new { message = "Venta no encontrada" });
                }

                return Ok(sale);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<SaleDto>> CreateSale([FromBody] CreateSaleDto createDto)
        {
            try
            {
                var sale = await _saleService.CreateSaleAsync(createDto);
                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("from-quote/{quoteId}")]
        public async Task<ActionResult<SaleDto>> CreateSaleFromQuote(int quoteId)
        {
            try
            {
                var sale = await _saleService.CreateSaleFromQuoteAsync(quoteId);
                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<SaleDto>> UpdateSaleStatus(int id, [FromBody] UpdateSaleStatusDto updateDto)
        {
            try
            {
                var sale = await _saleService.UpdateSaleStatusAsync(id, updateDto);
                return Ok(sale);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<SaleDto>>> GetUserSales(int userId)
        {
            try
            {
                // Verificar permisos
                var currentUserId = JwtHelper.GetUserIdFromToken(User);
                var userRole = User.FindFirst("Role")?.Value;

                if (userRole != "Admin" && currentUserId != userId)
                {
                    return Forbid();
                }

                var sales = await _saleService.GetSalesByUserAsync(userId);
                return Ok(sales);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTO adicional para Sales
    public class UpdateSaleStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }

        public string? Notes { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }
}