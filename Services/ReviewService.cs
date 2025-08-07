using Microsoft.EntityFrameworkCore;
using AGROPURE.Data;
using AGROPURE.Models.DTOs;
using AGROPURE.Models.Entities;
using AutoMapper;

namespace AGROPURE.Services
{
    public class ReviewService
    {
        private readonly AgroContext _context;
        private readonly IMapper _mapper;

        public ReviewService(AgroContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> CanUserReviewProductAsync(int userId, int productId)
        {
            // Verificar si el usuario ha comprado este producto
            var hasPurchased = await _context.Sales
                .AnyAsync(s => s.UserId == userId &&
                              s.ProductId == productId &&
                              s.Status == OrderStatus.Delivered);

            if (!hasPurchased)
                return false;

            // Verificar si ya ha hecho una reseña
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);

            return !hasReviewed;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createDto, int userId)
        {
            // Verificar que puede hacer la reseña
            var canReview = await CanUserReviewProductAsync(userId, createDto.ProductId);
            if (!canReview)
            {
                throw new InvalidOperationException("No puedes hacer una reseña de este producto. Debes haberlo comprado y no haber hecho una reseña previa.");
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
                .Include(r => r.Product)
                .FirstAsync(r => r.Id == review.Id);

            return _mapper.Map<ReviewDto>(createdReview);
        }
    }
}
