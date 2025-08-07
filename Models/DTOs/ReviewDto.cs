namespace AGROPURE.Models.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public bool CanReview { get; set; } // NUEVO: indica si el usuario puede hacer reseña
    }

    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [Required]
        public int ProductId { get; set; }
    }
}
