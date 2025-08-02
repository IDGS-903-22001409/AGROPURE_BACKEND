using AGROPURE.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AGROPURE.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Company { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public UserRole Role { get; set; } = UserRole.Customer;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}