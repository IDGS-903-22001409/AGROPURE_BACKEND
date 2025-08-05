using Microsoft.EntityFrameworkCore;
using AGROPURE.Data;
using AGROPURE.Helpers;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGROPURE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public ReviewsController(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<List<ReviewDto>>> GetProductReviews(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Where(r => r.ProductId == productId && r.IsApproved)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);
                return Ok(reviewDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ReviewDto>>> GetPendingReviews()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .Where(r => !r.IsApproved)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);
                return Ok(reviewDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createDto)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(User);

                // Verificar que el producto existe
                var product = await _context.Products.FindAsync(createDto.ProductId);
                if (product == null || !product.IsActive)
                {
                    return BadRequest(new { message = "Producto no encontrado" });
                }

                // Verificar que el usuario no haya hecho ya una reseña de este producto
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == createDto.ProductId);

                if (existingReview != null)
                {
                    return BadRequest(new { message = "Ya has hecho una reseña de este producto" });
                }

                var review = new Review
                {
                    UserId = userId,
                    ProductId = createDto.ProductId,
                    Rating = createDto.Rating,
                    Comment = createDto.Comment,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false // Requiere aprobación del admin
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Recargar con usuario incluido
                var createdReview = await _context.Reviews
                    .Include(r => r.User)
                    .FirstAsync(r => r.Id == review.Id);

                var reviewDto = _mapper.Map<ReviewDto>(createdReview);
                return CreatedAtAction(nameof(GetProductReviews), new { productId = createDto.ProductId }, reviewDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApproveReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound(new { message = "Reseña no encontrada" });
                }

                review.IsApproved = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Reseña aprobada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound(new { message = "Reseña no encontrada" });
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Reseña eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}