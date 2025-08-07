using AGROPURE.Models.DTOs;
using AGROPURE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AGROPURE.Data; 
using AGROPURE.Models.Entities;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly CostingService _costingService;
        private readonly AgroContext _context; 

        public ProductsController(ProductService productService, CostingService costingService, AgroContext context) 
        {
            _productService = productService;
            _costingService = costingService;
            _context = context;
        }

        [HttpPost("{id}/faqs")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductFaqDto>> AddProductFaq(int id, [FromBody] CreateProductFaqDto createDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || !product.IsActive)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                var faq = new ProductFaq
                {
                    ProductId = id,
                    Question = createDto.Question,
                    Answer = createDto.Answer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProductFaqs.Add(faq);
                await _context.SaveChangesAsync();

                var faqDto = new ProductFaqDto
                {
                    Id = faq.Id,
                    ProductId = faq.ProductId,
                    Question = faq.Question,
                    Answer = faq.Answer,
                    IsActive = faq.IsActive,
                    CreatedAt = faq.CreatedAt
                };

                return CreatedAtAction(nameof(GetProduct), new { id = faq.Id }, faqDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/faqs")]
        public async Task<ActionResult<List<ProductFaqDto>>> GetProductFaqs(int id)
        {
            try
            {
                var faqs = await _context.ProductFaqs
                    .Where(f => f.ProductId == id && f.IsActive)
                    .OrderBy(f => f.CreatedAt)
                    .Select(f => new ProductFaqDto
                    {
                        Id = f.Id,
                        ProductId = f.ProductId,
                        Question = f.Question,
                        Answer = f.Answer,
                        IsActive = f.IsActive,
                        CreatedAt = f.CreatedAt
                    })
                    .ToListAsync();

                return Ok(faqs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(createDto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] CreateProductDto updateDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateDto);
                return Ok(product);
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                return Ok(new { message = "Producto eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("calculate-price")]
        public async Task<ActionResult<PriceCalculationDto>> CalculatePrice([FromBody] CalculatePriceDto request)
        {
            try
            {
                var calculation = await _costingService.CalculateProductCostAsync(request.ProductId, request.Quantity);

                var baseTotal = calculation.UnitPrice * request.Quantity;
                var discount = baseTotal - calculation.TotalCost;

                var response = new PriceCalculationDto
                {
                    UnitPrice = calculation.UnitPrice,
                    TotalCost = calculation.TotalCost,
                    Discount = discount,
                    VolumeDiscountPercentage = calculation.VolumeDiscount
                };

                return Ok(response);
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
    }

    public class CalculatePriceDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class PriceCalculationDto
    {
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Discount { get; set; }
        public decimal VolumeDiscountPercentage { get; set; }
    }
}